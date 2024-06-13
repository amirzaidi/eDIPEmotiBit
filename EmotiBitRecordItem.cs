using System.Text.Json.Serialization;

namespace eDIPEmotiBit
{
    [Serializable]
    public class EmotiBitRecordItem(string _value)
    {
        [JsonInclude]
        public string time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");

        [JsonInclude]
        public string value = _value;
    }
}
