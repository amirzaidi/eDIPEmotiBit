using System.Text.Json;
using System.Text.Json.Serialization;

namespace eDIPEmotiBit
{
    [Serializable]
    public class EmotiBitRecords
    {
        [JsonInclude]
        public string start = "";

        [JsonInclude]
        public string end = "";

        [JsonInclude]
        public List<EmotiBitRecordItem> values = [];

        public string ToJson(JsonSerializerOptions? options = null) =>
            JsonSerializer.Serialize(this, options);

        internal void Clear()
        {
            start = "";
            end = "";
            values.Clear();
        }
    }
}
