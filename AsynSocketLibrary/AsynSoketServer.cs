using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace AsynSocketLibrary
{
    public class AsynSoketServer
    {
        public event Action<string> MessageReceived;
        public event Action<int> OnClientConnected;

        private IPAddress mIP;
        private int mPort;
        private TcpListener mTcpListener;
        List<TcpClient> mClients;
        
        private bool KeepRunning {  get; set; }
        
        public AsynSoketServer()
        {
            mClients = new List<TcpClient>();
        }

        public async Task StartListeningForIncomingConnection(IPAddress ipaddr = null, int port = 9001)
        {
            if (ipaddr == null)
            {
                ipaddr = IPAddress.Any;
            }
            if (port == 0)
            {
                port = 9001;
            }

            mIP = ipaddr;
            mPort = port;
            Debug.WriteLine($"IP Address: {mIP.ToString()} - Port: {mPort}");

            mTcpListener = new TcpListener(mIP, mPort);
            try
            {
                mTcpListener.Start();
                KeepRunning = true;

                while (KeepRunning)
                {
                    var returnedByAccept = await mTcpListener.AcceptTcpClientAsync();
                    mClients.Add(returnedByAccept);
                    Debug.WriteLine($"Client connected successfully - {mClients.Count}");
                    _=TakeCareOfTCPClient(returnedByAccept);
                    ClientCount(mClients.Count);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async Task TakeCareOfTCPClient(TcpClient returnedByAccept)
        {
            NetworkStream stream = null;
            StreamReader reader = null;
            StreamWriter writer = null;

            try
            {
                stream = returnedByAccept.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                char[] buff = new char[1024];

                while (KeepRunning)
                {
                    Debug.WriteLine("Ready to read");
                    int nRet = await reader.ReadAsync(buff, 0, buff.Length);

                    if (nRet == 0)
                    {
                        RemoveClient(returnedByAccept);
                        break;
                    }

                    string receivedText = new string(buff, 0, nRet);
                    Array.Clear(buff, 0, buff.Length);
                    OnMessageReceived(receivedText);

                }
            }
            catch (Exception e)
            {
                RemoveClient(returnedByAccept);
                Debug.WriteLine(e.ToString());
            }
        }

        private void RemoveClient(TcpClient tcpClient)
        {
            
            if (mClients.Contains(tcpClient))
            {
                mClients.Remove(tcpClient);
                ClientCount(mClients.Count);
                Debug.WriteLine($"Client removed, count {mClients.Count}");
            }
        }

        public void SendToAll(string mess)
        {
            string messSer = "Server: " + mess;
            if (string.IsNullOrEmpty(mess))
            {
                return;
            }
            try
            {
                byte[] buffMess = Encoding.ASCII.GetBytes(messSer);
                foreach (TcpClient user in mClients)
                {
                    user.GetStream().WriteAsync(buffMess, 0, buffMess.Length);
                    Debug.WriteLine("send");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public void StopServer()
        {
            try
            {
                if (mTcpListener != null)
                {
                    mTcpListener.Stop();
                }

                foreach (TcpClient user in mClients)
                {
                    user.Close();
                }
                mClients.Clear();
            }
            catch(SocketException e)
            {
                Debug.WriteLine(e);
            }
        }
        public void SendToClient(string mess, TcpClient targetClient)
        {
            string messSer = "Server: " + mess;
            if (string.IsNullOrEmpty(mess))
            {
                return;
            }
            try
            {
                byte[] buffMess = Encoding.ASCII.GetBytes(messSer);
                targetClient.GetStream().WriteAsync(buffMess, 0, buffMess.Length);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
        protected void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(message);
        }
        private void ClientCount(int client)
        {
            OnClientConnected?.Invoke(client);
        }

    }
}
