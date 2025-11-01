namespace ChatClient.WinForms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null!;
        private TextBox txtServer = null!;
        private NumericUpDown numPort = null!;
        private TextBox txtNick = null!;
        private Button btnConnect = null!;
        private Button btnDisconnect = null!;
        private ListBox lstMessages = null!;
        private TextBox txtMessage = null!;
        private Button btnSend = null!;
        private ListBox lstUsers = null!;
        private Label lblUsers = null!;
        private Label lblUsersCount = null!;
        private Label lblServer = null!;
        private Label lblPort = null!;
        private Label lblNick = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            this.Text = "Socket Chat (WinForms, .NET 8)";
            this.MinimumSize = new Size(820, 540);
            this.StartPosition = FormStartPosition.CenterScreen;

            lblServer = new Label { Text = "Sunucu", AutoSize = true, Location = new Point(12, 15) };
            txtServer = new TextBox { Location = new Point(70, 12), Width = 150 };

            lblPort = new Label { Text = "Port", AutoSize = true, Location = new Point(230, 15) };
            numPort = new NumericUpDown { Location = new Point(265, 12), Width = 80, Minimum = 1, Maximum = 65535, Value = 5000 };

            lblNick = new Label { Text = "Takma Ad", AutoSize = true, Location = new Point(360, 15) };
            txtNick = new TextBox { Location = new Point(430, 12), Width = 140 };

            btnConnect = new Button { Text = "Bağlan", Location = new Point(580, 10), Size = new Size(90, 28) };
            btnDisconnect = new Button { Text = "Kes", Location = new Point(680, 10), Size = new Size(90, 28) };

            lstMessages = new ListBox { Location = new Point(12, 50), Size = new Size(600, 360) };
            lblUsers = new Label { Text = "Kullanıcılar", AutoSize = true, Location = new Point(625, 50) };
            lblUsersCount = new Label { Text = "Kullanıcılar (0)", AutoSize = true, Location = new Point(625, 70) };
            lstUsers = new ListBox { Location = new Point(625, 90), Size = new Size(145, 320) };

            txtMessage = new TextBox { Location = new Point(12, 420), Size = new Size(600, 28) };
            btnSend = new Button { Text = "Gönder", Location = new Point(625, 418), Size = new Size(145, 30) };

            btnConnect.Click += btnConnect_Click;
            btnDisconnect.Click += btnDisconnect_Click;
            btnSend.Click += btnSend_Click;
            txtMessage.KeyDown += txtMessage_KeyDown;
            this.FormClosing += MainForm_FormClosing;

            Controls.AddRange(new Control[]
            {
                lblServer, txtServer, lblPort, numPort, lblNick, txtNick, btnConnect, btnDisconnect,
                lstMessages, lblUsers, lblUsersCount, lstUsers, txtMessage, btnSend
            });
        }
    }
}
