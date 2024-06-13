using System.Net.Sockets;
using System.Text;

namespace eDIPEmotiBit
{
    internal class UDPListener : IDisposable
    {
        private const int PORT = 12346;

        private readonly UdpClient mUdpClient = new(PORT);

        internal async Task LoopReceive(Func<string, Task> onReceive, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var result = await mUdpClient.ReceiveAsync(token);
                    await onReceive.Invoke(Encoding.ASCII.GetString(result.Buffer));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void Dispose()
        {
            mUdpClient.Close();
            mUdpClient.Dispose();
        }
    }
}
