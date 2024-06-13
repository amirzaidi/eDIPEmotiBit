namespace eDIPEmotiBit
{
    internal class EmotiBit
    {
        private static readonly string[] TAGS_BIOMETRIC = ["EA", "EL", "ER", "PI", "PR", "PG", "T0", "TH", "AX", "AY", "AZ", "GX", "GY", "GZ", "MX", "MY", "MZ", "SA", "SR", "SF", "HR", "BI", "H0"];
        private static readonly string[] TAGS_GENERAL = ["EI", "DC", "DO", "B%", "BV", "D%", "RD", "PI", "PO", "RS"];
        private static readonly string[] TAGS_COMPUTER = ["GL", "GS", "GB", "GA", "TL", "TU", "TX", "LM", "RB", "RE", "UN", "MH", "HE"];

        public readonly EmotiBitRecords mRecordBiometric = new();
        public readonly EmotiBitRecords mRecordGeneral = new();
        public readonly EmotiBitRecords mRecordTags = new();

        public bool isRecording = false;
        public bool isDebugMode = true;

        public float dataTimeout = 3f;
        public bool isReady = true;

        public Action onBatteryLevelLow;
        public Action onDataTimeoutReceived;
        public Action<string> onBiometricDataReceived;

        private float lastDataTime = 0;
        private bool isDataTimeout = false;
        private int lowBatteryLevel = 7;
        private bool bBatteryLow = false;
        private int batteryLevel = 100;
        private UDPListener myUDPClient;

        string toDebugLog = null;

        internal void OnNewData(string data)
        {
            lastDataTime = 0;
            isDataTimeout = false;

            if (isDebugMode)
            {
                Debug.Log(data);
            }

            if (isRecording)
            {
                try
                {
                    EmotiBitRecordItem recordItem = new EmotiBitRecordItem(data);

                    if (data.Contains("B%"))
                    {
                        string[] fields = data.Split(',');
                        if (fields.Length == 7)
                        {
                            bool successfullyParsed = int.TryParse(fields[6], out batteryLevel);
                            if (successfullyParsed)
                            {
                                if (batteryLevel < lowBatteryLevel)
                                {
                                    bBatteryLow = true;
                                    Debug.Log("Emotibit Battery Low!");
                                }
                            }
                        }
                    }

                    foreach (string tag in TAGS_BIOMETRIC)
                    {
                        if (recordItem.value.Contains(tag))
                        {
                            mRecordBiometric.values.Add(recordItem);
                            recordItemToSend = recordItem;
                            return;
                        }
                    }

                    foreach (string tag in TAGS_GENERAL)
                    {
                        if (recordItem.value.Contains(tag))
                        {
                            mRecordGeneral.values.Add(recordItem);
                            return;
                        }
                    }

                    foreach (string tag in TAGS_COMPUTER)
                    {
                        if (recordItem.value.Contains(tag))
                        {
                            mRecordTags.values.Add(recordItem);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error: {ex.Message}");
                }
            }
        }

        public void Start(string filePath)
        {

            if (isRecording) return;
            else
            {
                if (!isDataTimeout)
                {
                    mRecordBiometric.Clear();
                    mRecordGeneral.Clear();
                    mRecordTags.Clear();

                    string currentTime = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                    mRecordBiometric.start = currentTime;
                    mRecordGeneral.start = currentTime;
                    mRecordTags.start = currentTime;

                    isRecording = true;
                }
                else
                {
                    isDataTimeout = false;
                }
            }
        }

        public void Stop(string filePath)
        {
            Debug.Log(Directory.GetCurrentDirectory());

            if (!isRecording) return;

            isRecording = false;

            string currentTime = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
            mRecordBiometric.end = currentTime;
            mRecordGeneral.end = currentTime;
            mRecordTags.end = currentTime;

            if (mRecordBiometric.start.Length > 0)
            {
                if (filePath != null)
                {
                    string path = Directory.GetCurrentDirectory() + filePath + "/emotibit_biometric_data.json";
                    path = GetSafeFileName(path);

                    if (IsValidPath(path))
                    {
                        new FileInfo(path).Directory.Create();
                        StreamWriter writer = File.CreateText(path);
                        writer.WriteLine(mRecordBiometric.ToString());
                        writer.Close();
                    }
                }
            }

            if (mRecordGeneral.start.Length > 0)
            {
                if (filePath != null)
                {
                    string path = Directory.GetCurrentDirectory() + filePath + "/emotibit_general_data.json";
                    path = GetSafeFileName(path);

                    if (IsValidPath(path))
                    {
                        new FileInfo(path).Directory.Create();
                        StreamWriter writer = File.CreateText(path);
                        writer.WriteLine(mRecordGeneral.ToString());
                        writer.Close();
                    }
                }
            }

            if (mRecordTags.start.Length > 0)
            {
                if (filePath != null)
                {
                    string path = Directory.GetCurrentDirectory() + filePath + "/emotibit_computer_data.json";
                    path = GetSafeFileName(path);

                    if (IsValidPath(path))
                    {
                        new FileInfo(path).Directory.Create();
                        StreamWriter writer = File.CreateText(path);
                        writer.WriteLine(mRecordTags.ToString());
                        writer.Close();
                    }
                }
            }
        }

        public string GetSafeFileName(string Path)
        {

            string tmp_path_completado = Path;

            int c = 1;
            while (File.Exists(tmp_path_completado))
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(tmp_path_completado);
                if (fileName.Contains("@")) { fileName = fileName.Split('@')[0]; } // eliminar anterior indice
                fileName = String.Format("{0}{1}{2}", fileName, "@" + c.ToString(), System.IO.Path.GetExtension(tmp_path_completado));
                tmp_path_completado = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(tmp_path_completado), fileName);
                c++;
                if (c > 100) break;
            }

            return tmp_path_completado;

        }


        private void OnApplicationQuit()
        {

        }

        // Validates the given path to ensure it is a usable file path.
        public bool IsValidPath(string path)
        {
            bool isValid = !string.IsNullOrEmpty(path) && path.Length > 2 && path.IndexOfAny(Path.GetInvalidPathChars()) < 0;
            return isValid;
        }


        EmotiBitRecordItem recordItemToSend = null;
        void Update()
        {
            if (recordItemToSend != null)
            {
                onBiometricDataReceived?.Invoke(recordItemToSend.value);
                recordItemToSend = null;
            }

            if (bBatteryLow)
            {
                onBatteryLevelLow?.Invoke();
                bBatteryLow = false;
            }

            if (!isDataTimeout)
            {
                /*
                lastDataTime += Time.deltaTime;
                if (lastDataTime > dataTimeout)
                {
                    isDataTimeout = true;
                    onDataTimeoutReceived?.Invoke();
                    Debug.Log("Emotibit Data Timeout!");
                }
                */
            }

            isReady = !isDataTimeout;
        }
    }
}