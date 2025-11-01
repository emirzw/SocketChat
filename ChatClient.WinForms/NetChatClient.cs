using System.Net.Sockets;
using System.Text;

namespace ChatClient.WinForms;

public sealed class NetChatClient : IDisposable
{
    private TcpClient? _tcp;
    private NetworkStream? _ns;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public bool IsConnected => _tcp?.Connected == true;

    public event Action<string>? MessageReceived;
    public event Action<string[]>? UserListReceived;

    public async Task ConnectAsync(string host, int port, string nickname, CancellationToken ct)
    {
        _tcp = new TcpClient();
        await _tcp.ConnectAsync(host, port, ct);
        _tcp.NoDelay = true;

        _ns = _tcp.GetStream();
        _reader = new StreamReader(_ns, new UTF8Encoding(false), leaveOpen: true);
        _writer = new StreamWriter(_ns, new UTF8Encoding(false)) { AutoFlush = true };

        // Nick'i ayarla
        await SendAsync($"/nick {nickname}");
        _ = Task.Run(() => ReceiveLoopAsync(ct), ct);
    }

    public Task SendAsync(string text)
    {
        if (_writer is null) throw new InvalidOperationException("Bağlı değil.");
        return _writer.WriteLineAsync(text);
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        if (_reader is null) return;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var line = await _reader.ReadLineAsync(ct);
                if (line is null) break;

                if (line.StartsWith("#USERS "))
                {
                    var csv = line.Substring(7);
                    var arr = csv.Length == 0 ? Array.Empty<string>() : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    UserListReceived?.Invoke(arr);
                }
                else
                {
                    MessageReceived?.Invoke(line);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            MessageReceived?.Invoke($"[HATA] {ex.Message}");
        }
    }

    public void Dispose()
    {
        try { _ns?.Dispose(); } catch { }
        try { _tcp?.Close(); } catch { }
    }
}
