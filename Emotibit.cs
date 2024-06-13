using eDIPEmotiBit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class EmotibitRecordItem
{
    public string time;
    public string value;

    // Constructor that initializes a new record item with a value.
    public EmotibitRecordItem(string _value)
    {
        time = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
        value = _value;
    }
}

[Serializable]
public class EmotibitRecords
{
    public string start = "";
    public string end = "";
    public List<EmotibitRecordItem> values = new List<EmotibitRecordItem>();

    // Returns a JSON string representation of the record.
    public string ToString()
    {
        //return JsonUtility.ToJson(this);
        return "";
    }

    // Clears the records.
    public void Clear()
    {
        start = "";
        end = "";
        values.Clear();
    }
}

public class Emotibit /* : MonoBehaviour */
{
    string[] biometricTags = new string[] { "EA", "EL", "ER", "PI", "PR", "PG", "T0", "TH", "AX", "AY", "AZ", "GX", "GY", "GZ", "MX", "MY", "MZ", "SA", "SR", "SF", "HR", "BI", "H0" };
    string[] generalTags = new string[] { "EI", "DC", "DO", "B%", "BV", "D%", "RD", "PI", "PO", "RS" };
    string[] computerTags = new string[] { "GL", "GS", "GB", "GA", "TL", "TU", "TX", "LM", "RB", "RE", "UN", "MH", "HE" };

    public static Emotibit instance;
    public EmotibitRecords biometricRecord;
    public EmotibitRecords generalRecord;
    public EmotibitRecords computerRecord;
    public bool isRecording = false;
    public bool isDebugMode = false;

    public float dataTimeout = 3f;
    public bool isReady = true;

    public System.Action onBatteryLevelLow = null;
    public System.Action onDataTimeoutReceived = null;
    public System.Action<string> onBiometricDataReceived = null;

    private float lastDataTime = 0;
    private bool isDataTimeout = false;
    private int lowBatteryLevel = 7;
    private bool bBatteryLow = false;
    private int batteryLevel = 100;
    private UDPListener myUDPClient;

    void Start()
    {
        myUDPClient = new UDPListener();
        myUDPClient.OnReceive += OnNewData;
    }

    private void Awake()
    {
        instance = this;
    }

    string toDebugLog = null;
    void OnNewData(string data)
    {
        lastDataTime = 0;
        isDataTimeout = false;

        if (isDebugMode)
        {
            Debug.Log(data);
        }

        if (isRecording)
        {
            try {
                    EmotibitRecordItem recordItem = new EmotibitRecordItem(data);

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

                    foreach (string tag in biometricTags)
                    {
                        if (recordItem.value.Contains(tag))
                        {                          
                            biometricRecord.values.Add(recordItem);
                            recordItemToSend = recordItem;
                            return;
                        }
                    }

                    foreach (string tag in generalTags)
                    {
                        if (recordItem.value.Contains(tag))
                        {
                            generalRecord.values.Add(recordItem);
                            return;
                        }
                    }

                    foreach (string tag in computerTags)
                    {
                        if (recordItem.value.Contains(tag))
                        {
                            computerRecord.values.Add(recordItem);
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

    public void Play()
    {
        
        if (isRecording) return;
        else
        {
            if (!isDataTimeout)
            {
                biometricRecord.Clear();
                generalRecord.Clear();
                computerRecord.Clear();

                string currentTime = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                biometricRecord.start = currentTime;
                generalRecord.start = currentTime;
                computerRecord.start = currentTime;

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
        biometricRecord.end = currentTime;
        generalRecord.end = currentTime;
        computerRecord.end = currentTime;

        if (biometricRecord.start.Length > 0)
        {
            if (filePath != null)
            {
                string path = Directory.GetCurrentDirectory() + filePath + "/emotibit_biometric_data.json";
                path = GetSafeFileName(path);

                if (IsValidPath(path))
                {
                    new FileInfo(path).Directory.Create();
                    StreamWriter writer = File.CreateText(path);
                    writer.WriteLine(biometricRecord.ToString());
                    writer.Close();
                }
            }
        }

        if (generalRecord.start.Length > 0)
        {
            if (filePath != null)
            {
                string path = Directory.GetCurrentDirectory() + filePath + "/emotibit_general_data.json";
                path = GetSafeFileName(path);

                if (IsValidPath(path))
                {
                    new FileInfo(path).Directory.Create();
                    StreamWriter writer = File.CreateText(path);
                    writer.WriteLine(generalRecord.ToString());
                    writer.Close();
                }
            }
        }

        if (computerRecord.start.Length > 0)
        {
            if (filePath != null)
            {
                string path = Directory.GetCurrentDirectory() + filePath + "/emotibit_computer_data.json";
                path = GetSafeFileName(path);

                if (IsValidPath(path))
                {
                    new FileInfo(path).Directory.Create();
                    StreamWriter writer = File.CreateText(path);
                    writer.WriteLine(computerRecord.ToString());
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


    EmotibitRecordItem recordItemToSend = null;
    void Update()
    {
        if (recordItemToSend != null) {
            onBiometricDataReceived?.Invoke(recordItemToSend.value);
            recordItemToSend = null;
        }

        if (bBatteryLow) {
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
