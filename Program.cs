// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var cts = new CancellationTokenSource();
var console = Task.Run(async () =>
{
    while (Console.ReadKey(true).KeyChar != 'q')
    {
    }

    await cts.CancelAsync();
});

using (var udp = new UDPListener())
{
    await udp.LoopReceive(cts.Token);
    await console;
}

Console.WriteLine("Goodbye, World!");
