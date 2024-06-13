namespace eDIPEmotiBit
{
    internal class Debug
    {
        internal static void Log(string message)
        {
            var t = DateTime.Now.ToString("HH:mm:ss - ");
            Console.WriteLine($"{t} {message}");
        }
    }
}
