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
//emotiBit.OnTimeout += () => Debug.Log("Timeout");
//emotiBit.OnBiometricData += _ => Debug.Log($"Biometric {_}");

using (var udp = new UDPListener())
{
    await udp.LoopReceive(emotiBit.OnData, cts.Token);
    await console;
}
