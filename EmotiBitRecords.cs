namespace eDIPEmotiBit
{
    [Serializable]
    internal class EmotibitRecords
    {
        public string start = "";
        public string end = "";
        public List<EmotiBitRecordItem> values = new List<EmotiBitRecordItem>();

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
}
