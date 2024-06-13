using eDIPEmotiBit;

var cts = new CancellationTokenSource();
var console = Task.Run(async () =>
{
    while (Console.ReadKey(true).KeyChar != 'q')
    {
    }

    await cts.CancelAsync();
});

var emotiBit = new EmotiBit();
emotiBit.SetPath("log");
emotiBit.OnBatteryLow += () => Debug.Log("Battery low");

using (var udp = new UDPListener())
{
    await udp.LoopReceive(emotiBit.OnData, cts.Token);
    await console;
}
