using System.Net.Sockets;
using System.Text;

internal class UDPListener : IDisposable
{
    private const int PORT = 12346;

    public event Action<string>? OnReceive;
    private readonly UdpClient mUdpClient = new(PORT);

    internal async Task LoopReceive(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var result = await mUdpClient.ReceiveAsync(token);
                OnReceive?.Invoke(Encoding.ASCII.GetString(result.Buffer));
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
