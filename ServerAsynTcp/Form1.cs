using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerAsynTcp
{
    public partial class Form1 : Form
    {
        AsynSocketLibrary.AsynSoketServer mServer;
        

        public Form1()
        {
            InitializeComponent();
            mServer = new AsynSocketLibrary.AsynSoketServer();
            mServer.MessageReceived += MessageReceived;
            mServer.OnClientConnected += ClientCount;

        }

        private async void btnAccept_Click(object sender, EventArgs e)
        {
            await Task.Run(() => mServer.StartListeningForIncomingConnection());
        }
        

        private void btnSendAll_Click(object sender, EventArgs e)
        {
            mServer.SendToAll(txtMess.Text.Trim());
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            mServer.StopServer();
        }
        private void MessageReceived(string message)
        {
            this.Invoke((MethodInvoker)delegate
            {
                rtbMess.AppendText(message + Environment.NewLine); 
                rtbMess.ScrollToCaret();
            });
        }
        
        private void SendMessageToClient(TcpClient client)
        {
            string message = txtMess.Text.ToString();
            mServer.SendToClient(message,client);
        }

        private void dtgClient_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void ClientCount(int client)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtStatus.Text = client.ToString();
            });
        }
      

    }
}
