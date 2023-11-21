using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsynSocketLibrary
{
    public class AsynSocketClient
    {
        private TcpClient tcpClient; 
        private StreamReader reader;
        private StreamWriter writer;
        

        public event Action<string> MessageReceived;
        public event Action<string> ServerDisconnected;
        


        public async Task ConnectToServerAsync(string ipAddress, int port, string username)
        {
            try
            {
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(ipAddress, port);
                Debug.WriteLine("Connected to Server");
                NetworkStream stream = tcpClient.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                await ReceiveDataAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task ReceiveDataAsync()
        {
            try
            {
                while (tcpClient.Connected)
                {
                    char[] buffer = new char[1024];
                    int bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        ServerDisconnected?.Invoke("Server disconnected.");
                        break;
                    }

                    string receivedText = new string(buffer, 0, bytesRead);
                    MessageReceived?.Invoke(receivedText);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task SendDataAsync(string message, string username)
        {
            string mess = username + ":" + message;
            try
            {
                if (tcpClient != null && tcpClient.Connected)
                {
                    byte[] messBytes = Encoding.ASCII.GetBytes(mess);
                    await tcpClient.GetStream().WriteAsync(messBytes, 0, messBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Disconnect()
        {
            try
            {
                if (tcpClient != null)
                {
                    tcpClient.Close();
                }

                reader?.Dispose();
                writer?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
