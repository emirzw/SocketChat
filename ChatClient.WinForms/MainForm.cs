using System.Text;

namespace ChatClient.WinForms;

public partial class MainForm : Form
{
    private NetChatClient? _client;
    private CancellationTokenSource? _cts;

    public MainForm()
    {
        InitializeComponent();
        txtServer.Text = "127.0.0.1";
        numPort.Value = 5000;
        txtNick.Text = $"User{Random.Shared.Next(1000, 9999)}";
        ToggleUi(false);
    }

    private void ToggleUi(bool connected)
    {
        btnConnect.Enabled = !connected;
        btnDisconnect.Enabled = connected;
        txtServer.Enabled = !connected;
        numPort.Enabled = !connected;
        txtNick.Enabled = !connected;
        txtMessage.Enabled = connected;
        btnSend.Enabled = connected;
    }

    private void SafeUi(Action a)
    {
        if (InvokeRequired) BeginInvoke(a);
        else a();
    }

    private async void btnConnect_Click(object sender, EventArgs e)
    {
        var host = txtServer.Text.Trim();
        var port = (int)numPort.Value;
        var nick = txtNick.Text.Trim();
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(nick))
        {
            MessageBox.Show("Sunucu ve takma ad gerekli.");
            return;
        }

        try
        {
            _cts = new CancellationTokenSource();
            _client = new NetChatClient();
            _client.MessageReceived += s => SafeUi(() => AppendMessage(s));
            _client.UserListReceived += arr => SafeUi(() => UpdateUsers(arr));

            await _client.ConnectAsync(host, port, nick, _cts.Token);
            AppendMessage($"[Baðlandý] {host}:{port} olarak {nick}");
            ToggleUi(true);
            txtMessage.Focus();
        }
        catch (Exception ex)
        {
            AppendMessage("[HATA] " + ex.Message);
            _cts?.Cancel();
            _client?.Dispose();
            _client = null;
            ToggleUi(false);
        }
    }

    private void UpdateUsers(string[] users)
    {
        lstUsers.BeginUpdate();
        lstUsers.Items.Clear();
        lstUsers.Items.AddRange(users.Cast<object>().ToArray());
        lstUsers.EndUpdate();
        lblUsersCount.Text = $"Kullanýcýlar ({users.Length})";
    }

    private void AppendMessage(string msg)
    {
        var line = $"[{DateTime.Now:HH:mm}] {msg}";
        lstMessages.Items.Add(line);
        lstMessages.TopIndex = lstMessages.Items.Count - 1; // autoscroll
    }

    private async void btnSend_Click(object sender, EventArgs e)
    {
        var text = txtMessage.Text.Trim();
        if (string.IsNullOrEmpty(text) || _client is null) return;

        try
        {
            await _client.SendAsync(text);
            txtMessage.Clear();
            txtMessage.Focus();
        }
        catch (Exception ex)
        {
            AppendMessage("[HATA] " + ex.Message);
        }
    }

    private async void txtMessage_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !e.Shift)
        {
            e.SuppressKeyPress = true;
            await Task.Yield();
            btnSend.PerformClick();
        }
    }

    private void btnDisconnect_Click(object sender, EventArgs e)
    {
        Disconnect();
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Disconnect();
    }

    private void Disconnect()
    {
        try { _cts?.Cancel(); } catch { }
        try { _client?.Dispose(); } catch { }
        _client = null;
        _cts = null;
        ToggleUi(false);
        AppendMessage("[Baðlantý kapatýldý]");
    }
}
