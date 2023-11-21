using AsynSocketLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ClientAsynTcp
{
    public partial class Form1 : Form
    {
        private const string ConnectionString = "Data Source=DESKTOP-096NAA0;Initial Catalog=ChatDatabase;Integrated Security=True;MultipleActiveResultSets=True";
        private AsynSocketLibrary.AsynSocketClient socketClient;
        private string username;
        public string Username
        {
            get { return username; }
            set { username = value;
                txtUser.Text = value;
                  }
        }

        public Form1()
        {
            InitializeComponent();

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            
        }


        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (socketClient != null)
            {
                await Task.Run(() => socketClient.SendDataAsync(txtMess.Text, txtUser.Text.ToString()));
                txtMess.Clear();
            }
        }

        private void MessageReceived(string message)
        {
            
            this.Invoke((MethodInvoker)delegate
            {

                rtbMess.Text += message + "\n";
                rtbMess.SelectionStart = rtbMess.Text.Length;
                rtbMess.ScrollToCaret();



            });
        }
        private void ServerDisconnected(string message)
        {
            
            this.Invoke((MethodInvoker)delegate
            {
                MessageBox.Show("Disconnected from server: " + message, "Server Disconnected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            });
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            socketClient.Disconnect();
            try
            {

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("UPDATE Users SET Status = 'offline' WHERE Username = @Username", connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi thiết lập trạng thái 'online': {ex}");
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                socketClient = new AsynSocketLibrary.AsynSocketClient();
                socketClient.MessageReceived += MessageReceived;
                socketClient.ServerDisconnected += ServerDisconnected;

                await socketClient.ConnectToServerAsync("127.0.0.1", 9001,txtUser.Text.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void txtIP_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
