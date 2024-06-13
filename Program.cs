using eDIPEmotiBit;

Console.WriteLine("Hello, World!");

var cts = new CancellationTokenSource();
var console = Task.Run(async () =>
{
    while (Console.ReadKey(true).KeyChar != 'q')
    {
    }

    await cts.CancelAsync();
});

var emotiBit = new EmotiBit
{
    onBatteryLevelLow = () => Debug.Log("Low battery"),
    onDataTimeoutReceived = () => Debug.Log("Timeout data"),
    onBiometricDataReceived = _ => Debug.Log($"Biometric {_}"),
};

using (var udp = new UDPListener())
{
    udp.OnReceive += emotiBit.OnNewData;

    emotiBit.Start("/log");
    await udp.LoopReceive(cts.Token);
    emotiBit.Stop("/log");

    await console;
}

Console.WriteLine("Goodbye, World!");
