using JCommon;
using JCommon.FileDatabase;
using JCommon.FileDatabase.IO;
using JTcpNetwork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Tests
{
    public partial class ChatExample : Form
    {
        public ChatExample()
        {
            InitializeComponent();
        }

        #region SIMPLE CHAT

        #region CHAT SERVER
        bool serverStarted = false;
        private void StartServer_Click(object sender, System.EventArgs e)
        {
            if (!serverStarted)
            {
                serverStarted = true;
                NetworkServer.RegisterHandler(InternalMessages.CONNECTED, OnClientConnected);
                NetworkServer.RegisterHandler(InternalMessages.DISCONNECT, OnClientDisconected);
                NetworkServer.RegisterHandler(100, OnServerGetMsg);
                NetworkServer.Start(1985);
                richTextBox2.Text = "Server started\n";
            }
        }

        private void OnServerGetMsg(NetworkMessage netMsg)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => OnServerGetMsg(netMsg)));
                return;
            }
            ChatMsgPacket packet = netMsg.ReadMessage<ChatMsgPacket>();
            if (packet != null)
            {
                packet.ClientId = netMsg.conn.connectionId;
                NetworkServer.SendToAll(100, packet);
                richTextBox2.AppendText(string.Format("Client[{0}] sent one msg\n", netMsg.conn.connectionId));
            }
            richTextBox2.SelectionStart = richTextBox2.Text.Length;
            richTextBox2.ScrollToCaret();
        }

        private void OnClientDisconected(NetworkMessage netMsg)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => OnClientDisconected(netMsg)));
                return;
            }
            richTextBox2.AppendText(string.Format("Client Disconnected ClientId[{0}]\n", netMsg.conn.connectionId));
            richTextBox2.SelectionStart = richTextBox2.Text.Length;
            richTextBox2.ScrollToCaret();
        }

        private void OnClientConnected(NetworkMessage netMsg)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => OnClientConnected(netMsg)));
                return;
            }

            richTextBox2.AppendText(string.Format("Client Connected IP[{0}] ClientId[{1}]\n", netMsg.conn.IP, netMsg.conn.connectionId));
            richTextBox2.SelectionStart = richTextBox2.Text.Length;
            richTextBox2.ScrollToCaret();
        }

        #endregion

        #region CHAT CLIENT
 
        private void OnClientConnected_Click(object sender, EventArgs e)
        {
            if(!NetworkClient.Connected)
            {
                NetworkClient.RegisterHandler(InternalMessages.CONNECTED, OnConnectedToServer);
                NetworkClient.RegisterHandler(InternalMessages.DISCONNECT, OnDisconectedFromServer);
                NetworkClient.RegisterHandler(100, OnClientChatRecived);
                NetworkClient.Start(1985);
            }else
            {
                button1.Text = "Connect";
                richTextBox1.Text = "";
                richTextBox1.AppendText("Disconected from Server!\n");
                NetworkClient.Stop();
            }
        }

        private void OnClientChatRecived(NetworkMessage netMsg)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => OnClientChatRecived(netMsg)));
                return;
            }
            ChatMsgPacket packet = netMsg.ReadMessage<ChatMsgPacket>();
            if (packet != null)
            {
                richTextBox1.AppendText(string.Format("[{0}] - [{1}]\n", packet.ClientId, packet.Msg));
            }
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void OnDisconectedFromServer(NetworkMessage netMsg)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => OnDisconectedFromServer(netMsg)));
                return;
            }
            button1.Text = "Connect";
            richTextBox1.Text = "";
            richTextBox1.AppendText("Disconected from Server!\n");
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void OnConnectedToServer(NetworkMessage netMsg)
        {
            if (InvokeRequired) {
                Invoke(new MethodInvoker(() => OnConnectedToServer(netMsg) ));
                return;
            }
            button1.Text = "Disconnect";
            richTextBox1.AppendText("Connected to Server!\n");
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            if(CDManager.IsCoolingDown())
            {
                richTextBox1.AppendText("Slow down!\n");
            }
            var msg = textBox1.Text;

            if (NetworkClient.Connected)
            {
                if(!msg.ValidLength(1, 100))
                {
                    richTextBox1.AppendText("Message lenght must be minimum 1 and maximum 100 characters.\n");
                    return;
                }
                NetworkClient.Send(100, new ChatMsgPacket() { Msg = msg });
                textBox1.Text = "";
            }
            else
            {
                richTextBox1.AppendText("Not connected to the server!\n");
            }
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }
        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Button3_Click(null, null);
        }
        #endregion

        #endregion
    }
}
