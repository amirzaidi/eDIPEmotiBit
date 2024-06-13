using System.Text.Json;

namespace eDIPEmotiBit
{
    [Serializable]
    internal class EmotiBitRecords
    {
        internal string start = "";
        internal string end = "";
        internal List<EmotiBitRecordItem> values = [];

        public override string ToString() =>
            JsonSerializer.Serialize(this);

        internal void Clear()
        {
            start = "";
            end = "";
            values.Clear();
        }
    }
}
