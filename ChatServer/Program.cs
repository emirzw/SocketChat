using System.Net;
using System.Text;

namespace ChatServer;

internal static class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        int port = 5000;
        if (args.Length == 1 && int.TryParse(args[0], out var p)) port = p;

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

        var server = new Server(IPAddress.Any, port);
        Console.WriteLine($"[Server] {DateTime.Now:T} Port {port} dinleniyor. Kapatmak için Ctrl+C.");
        await server.StartAsync(cts.Token);
        Console.WriteLine("[Server] Kapatılıyor...");
    }
}
