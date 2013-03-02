using System;
using System.Windows.Forms;
using UDPCOMMON;

namespace UDPNATCLIENT
{
    public partial class frmClient : Form
    {
        private Client _client;

        public frmClient()
        {
            InitializeComponent();
        }

        private void frmClient_Load(object sender, EventArgs e)
        {
            _client = new Client {OnWriteMessage = WriteLog, OnUserChanged = OnUserChanged};
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _client.Login(textBox2.Text, "");
            _client.Start();
        }

        private void WriteLog(string msg)
        {
            listBox2.Items.Add(msg);
            listBox2.SelectedIndex = listBox2.Items.Count - 1;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_client != null)
            {
                User user = listBox1.SelectedItem as User;
                _client.HolePunching(user);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_client != null) _client.DownloadUserList();
        }

        private void frmClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_client != null) _client.Logout();
        }

        private void OnUserChanged(UserCollection users)
        {
            listBox1.DisplayMember = "FullName";
            listBox1.DataSource = null;
            listBox1.DataSource = users;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            P2P_TalkMessage msg = new P2P_TalkMessage(textBox1.Text);
            User user = listBox1.SelectedItem as User;
            _client.SendMessage(msg, user);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
        }
    }
}
