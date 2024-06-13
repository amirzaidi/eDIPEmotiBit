namespace eDIPEmotiBit
{
    internal class AppEmotiBit
    {
        public const int MAX_MESSAGES = 7;

        private readonly Queue<string> mMessageQueue = new();
        private readonly Queue<string> mDataQueue = new();

        internal AppEmotiBit()
        {
            Emotibit.instance.onBatteryLevelLow = () =>
            {
                LogMessage(DateTime.Now.ToString("HH:mm:ss - ") + "Emotibit - low battery!!!");
            };

            Emotibit.instance.onDataTimeoutReceived = () =>
            {
                LogMessage(DateTime.Now.ToString("HH:mm:ss - ") + "Emotibit - timeout data!!!");
            };


            Emotibit.instance.onBiometricDataReceived = (x) =>
            {
                LogData(DateTime.Now.ToString("HH:mm:ss - ") + x);
            };
        }

        internal void Start()
        {
            Emotibit.instance.Play();
            LogMessage("Saving data on: \n" + Directory.GetCurrentDirectory() + "/log");
        }

        internal void Stop()
        {
            Emotibit.instance.Stop("/log");
            LogMessage("Saved data on: \n" + Directory.GetCurrentDirectory() + "/log");
        }

        internal void LogMessage(string message)
        {
            if (mMessageQueue.Count >= MAX_MESSAGES)
            {
                mMessageQueue.Dequeue();
            }

            mMessageQueue.Enqueue(message);
        }

        internal void LogData(string message)
        {
            if (mDataQueue.Count >= MAX_MESSAGES)
            {
                mDataQueue.Dequeue();
            }

            mDataQueue.Enqueue(message);
        }
    }

}
