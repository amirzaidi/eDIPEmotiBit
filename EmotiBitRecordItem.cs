namespace eDIPEmotiBit
{
    [Serializable]
    internal class EmotiBitRecordItem(string _value)
    {
        public string time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
        public string value = _value;
    }
}
