using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer;

public sealed class Server
{
    private readonly IPAddress _ip;
    private readonly int _port;
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<string, ClientConnection> _clients = new();
    private int _userSeq = 1;

    public Server(IPAddress ip, int port)
    {
        _ip = ip;
        _port = port;
        _listener = new TcpListener(_ip, _port);
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _listener.Start();
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var tcp = await _listener.AcceptTcpClientAsync(ct);
                _ = HandleNewClientAsync(tcp, ct);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            foreach (var c in _clients.Values) c.Dispose();
            _listener.Stop();
        }
    }

    private async Task HandleNewClientAsync(TcpClient tcp, CancellationToken serverCt)
    {
        var conn = new ClientConnection(tcp);
        string nick = $"User{Interlocked.Increment(ref _userSeq)}";

        conn.Nickname = nick;
        _clients[nick] = conn;

        Console.WriteLine($"[JOIN] {conn.Nickname} bağlandı from {conn.RemoteEndPoint}");

        await conn.SendAsync("Sunucuya hoş geldin! /help ile komutları gör.\n" +
                             $"Takma adın şimdilik: {nick} — değiştirmek için: /nick YeniAd");
        await BroadcastAsync($"* {nick} sohbete katıldı *", exclude: nick);
        await BroadcastUsersAsync();

        try
        {
            await foreach (var line in conn.ReadLinesAsync(serverCt))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("/"))
                {
                    await HandleCommandAsync(conn, line, serverCt);
                }
                else
                {
                    await BroadcastAsync($"{conn.Nickname}: {line}", exclude: null);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERR] {conn.Nickname}: {ex.Message}");
        }
        finally
        {
            _clients.TryRemove(conn.Nickname, out _);
            Console.WriteLine($"[LEAVE] {conn.Nickname} ayrıldı");
            await BroadcastAsync($"* {conn.Nickname} sohbetten ayrıldı *", exclude: null);
            await BroadcastUsersAsync();
            conn.Dispose();
        }
    }

    private async Task HandleCommandAsync(ClientConnection conn, string raw, CancellationToken ct)
    {
        var parts = raw.Trim().Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToLowerInvariant();

        switch (cmd)
        {
            case "/help":
                await conn.SendAsync("Komutlar:\n" +
                    "/nick YeniAd    -> Takma ad değiştir\n" +
                    "/w Kullanıcı Mesaj -> Özel mesaj (whisper)\n" +
                    "/list           -> Kullanıcıları listele\n" +
                    "/help           -> Bu ekran");
                break;

            case "/nick":
                if (parts.Length < 2) { await conn.SendAsync("Kullanım: /nick YeniAd"); break; }
                var newNick = parts[1].Trim();
                if (string.IsNullOrWhiteSpace(newNick) || newNick.Contains(',') || newNick.StartsWith("#"))
                {
                    await conn.SendAsync("Geçersiz takma ad.");
                    break;
                }
                if (_clients.ContainsKey(newNick))
                {
                    await conn.SendAsync("Bu takma ad kullanımda.");
                    break;
                }
                var oldNick = conn.Nickname;
                if (_clients.TryRemove(oldNick, out _))
                {
                    conn.Nickname = newNick;
                    _clients[newNick] = conn;
                    await BroadcastAsync($"* {oldNick} -> {newNick} olarak adını değiştirdi *", exclude: null);
                    await BroadcastUsersAsync();
                }
                break;

            case "/w":
                if (parts.Length < 3) { await conn.SendAsync("Kullanım: /w HedefKullanıcı Mesaj"); break; }
                var target = parts[1];
                var msg = parts[2];
                if (_clients.TryGetValue(target, out var dest))
                {
                    await dest.SendAsync($"(özel) {conn.Nickname}: {msg}");
                    await conn.SendAsync($"(özel-> {target}) {msg}");
                }
                else
                {
                    await conn.SendAsync("Hedef kullanıcı yok/çevrimdışı.");
                }
                break;

            case "/list":
                await SendUsersAsync(conn);
                break;

            default:
                await conn.SendAsync("Bilinmeyen komut. /help");
                break;
        }
    }

    private Task SendUsersAsync(ClientConnection to)
    {
        var names = string.Join(",", _clients.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        // Özel protokol satırı (istemci bu satırı yakalayıp kullanıcı listesini güncelliyor)
        return to.SendAsync("#USERS " + names);
    }

    private Task BroadcastUsersAsync()
    {
        var names = string.Join(",", _clients.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        return BroadcastAsync("#USERS " + names, exclude: null);
    }

    private async Task BroadcastAsync(string line, string? exclude)
    {
        var tasks = new List<Task>(_clients.Count);
        foreach (var kv in _clients)
        {
            if (exclude != null && kv.Key == exclude) continue;
            tasks.Add(kv.Value.SendAsync(line));
        }
        try { await Task.WhenAll(tasks); } catch { /* tekil istemci hataları yutulur */ }
        Console.WriteLine($"[BCAST] {line}");
    }

    private sealed class ClientConnection : IDisposable
    {
        private readonly TcpClient _tcp;
        private readonly NetworkStream _ns;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        public string Nickname { get; set; } = "User";
        public EndPoint? RemoteEndPoint => _tcp.Client.RemoteEndPoint;

        public ClientConnection(TcpClient tcp)
        {
            _tcp = tcp;
            _tcp.NoDelay = true;
            _ns = _tcp.GetStream();
            _reader = new StreamReader(_ns, new UTF8Encoding(false), leaveOpen: true);
            _writer = new StreamWriter(_ns, new UTF8Encoding(false)) { AutoFlush = true };
        }

        public async IAsyncEnumerable<string> ReadLinesAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _tcp.Connected)
            {
                var line = await _reader.ReadLineAsync(ct);
                if (line is null) yield break;
                yield return line;
            }
        }

        public async Task SendAsync(string line)
        {
            await _sendLock.WaitAsync();
            try
            {
                await _writer.WriteLineAsync(line);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public void Dispose()
        {
            try { _ns.Dispose(); } catch { }
            try { _tcp.Close(); } catch { }
            _sendLock.Dispose();
        }
    }
}
