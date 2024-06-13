namespace eDIPEmotiBit
{
    internal class Debug
    {
        internal static void Log(string message)
        {
            var t = DateTime.UtcNow.ToString("HH:mm:ss.ffff");
            Console.WriteLine($"[{t}] {message}");
        }
    }
}
