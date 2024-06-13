using System.Text.Json;

namespace eDIPEmotiBit
{
    internal class EmotiBit
    {
        private const int LOW_BATTERY_LEVEL = 7;
        private const float TIMEOUT_SECONDS = 3f;

        private static readonly JsonSerializerOptions JSON_OPTIONS = new()
        {
            WriteIndented = true,
        };

        internal readonly struct TagList
        {
            internal readonly string Name;
            internal readonly string[] Tags;
            internal readonly EmotiBitRecords Records;

            internal TagList(string name, string[] tags)
            {
                Name = name;
                Tags = tags;
                Records = new();
            }
        }

        private readonly TagList[] mTagLists = [
            new(
                "Biometric",
                ["EA", "EL", "ER", "PI", "PR", "PG", "T0", "TH", "AX", "AY", "AZ", "GX", "GY", "GZ", "MX", "MY", "MZ", "SA", "SR", "SF", "HR", "BI", "H0"]
            ),
            new(
                "General",
                ["EI", "DC", "DO", "B%", "BV", "D%", "RD", "PI", "PO", "RS"]
            ),
            new(
                "Computer",
                ["GL", "GS", "GB", "GA", "TL", "TU", "TX", "LM", "RB", "RE", "UN", "MH", "HE"]
            ),
        ];

        public bool isRecording = false;
        public bool isDebugMode = true;

        public event Action? OnBatteryLow;
        public event Action? OnTimeout;
        public event Action<string>? OnBiometricData;

        private bool isDataTimeout = false;
        private float lastDataTime = 0;

        private int mBatteryLevel = 100;

        internal void OnData(string data)
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
                    var recordItem = new EmotiBitRecordItem(data);
                    if (data.Contains("B%"))
                    {
                        string[] fields = data.Split(',');
                        if (fields.Length == 7)
                        {
                            if (int.TryParse(fields[6], out mBatteryLevel))
                            {
                                if (mBatteryLevel < LOW_BATTERY_LEVEL)
                                {
                                    Debug.Log("Emotibit Battery Low!");
                                    OnBatteryLow?.Invoke();
                                }
                            }
                        }
                    }

                    foreach (var tagList in mTagLists)
                    {
                        foreach (var tag in tagList.Tags)
                        {
                            if (recordItem.value.Contains(tag))
                            {
                                tagList.Records.values.Add(recordItem);
                                if (tagList.Name == mTagLists[0].Name)
                                {
                                    OnBiometricData?.Invoke(recordItem.value);
                                }
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error: {ex.Message}");
                    throw;
                }
            }
        }

        public void Start(string filePath)
        {
            if (!isRecording)
            {
                if (!isDataTimeout)
                {
                    string currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                    foreach (var tagList in mTagLists)
                    {
                        tagList.Records.Clear();
                        tagList.Records.start = currentTime;
                    }

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
            if (!isRecording)
            {
                return;
            }

            isRecording = false;

            string currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
            foreach (var tagList in mTagLists)
            {
                tagList.Records.end = currentTime;

                if (tagList.Records.start.Length > 0 && filePath != null)
                {
                    string path = Directory.GetCurrentDirectory() + filePath + $"/emotibit_{tagList.Name.ToLower()}_data.json";
                    path = GetSafeFileName(path);
                    if (IsValidPath(path))
                    {
                        new FileInfo(path).Directory!.Create();
                        StreamWriter writer = File.CreateText(path);
                        var r = tagList.Records.ToJson(JSON_OPTIONS);
                        writer.WriteLine(r);
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


        // Validates the given path to ensure it is a usable file path.
        public static bool IsValidPath(string path) =>
            !string.IsNullOrEmpty(path)
            && path.Length > 2
            && path.IndexOfAny(Path.GetInvalidPathChars()) < 0;


        /*
        EmotiBitRecordItem recordItemToSend = null;
        void Update()
        {
            if (!isDataTimeout)
            {
                lastDataTime += Time.deltaTime;
                if (lastDataTime > dataTimeout)
                {
                    isDataTimeout = true;
                    onDataTimeoutReceived?.Invoke();
                    Debug.Log("Emotibit Data Timeout!");
                }
            }

            isReady = !isDataTimeout;
        }
        */
    }
}
