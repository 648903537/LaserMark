using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using com.amtec.configurations;
using System.Reflection;
using System.IO;
using com.amtec.forms;

namespace com.amtec.action
{
    class SocketServerHandler
    {
        private MainView mv;
        public SocketServerHandler(MainView mv1)
        {
            mv = mv1;
        }

        public IPEndPoint tcplisener;//将网络端点表示为IP地址和端口号
        public bool listen_flag = false;
        public Socket read;
        public Thread accept;//创建并控制线程
        public Thread monitor;//创建并控制线程
        public ManualResetEvent AcceptDone = new ManualResetEvent(false); //连接的信号
        public string _HostName = "";
        public string _IpAddress = "";

        internal string IpAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(_IpAddress))
                    return _IpAddress;

                var ips = Dns.GetHostAddresses(HostName);
                foreach (var ip in ips)
                {
                    if (ip.IsIPv6LinkLocal)
                        continue;

                    return ip.ToString();
                }
                return "";
            }
        }
        internal string HostName
        {
            get
            {
                return !string.IsNullOrEmpty(_HostName) ? _HostName : Dns.GetHostName();
            }
        }
        /// <summary>
        /// Open port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenPort(string ipAdress, string portName)
        {
            string ipaddress = ipAdress;
            string port = portName;
            System.Net.IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            string strIP = IpAddress;
            IPAddress ip = IPAddress.Parse(ipaddress.Trim());
            //用指定ip和端口号初始化
            tcplisener = new IPEndPoint(ip, Convert.ToInt32(port.Trim()));
            //创建一个socket对象
            read = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                read.Bind(tcplisener); //绑定
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message + "," + ex.StackTrace);
                mv.SetConnectionText(1, "Socket server run error");
                return;
            }
            ///收到客户端的连接，建立新的socket并接受信息
            read.Listen(500); //开始监听            
            mv.SetConnectionText(0, "Server run success,  Wait client connection");
            accept = new Thread(new ThreadStart(Listen));
            accept.Start();
            monitor = new Thread(new ThreadStart(SendHeartPackage));
            //if (mv.config.SendHeartPackage == "Y")
            //{
            //    monitor.Start();
            //}
            GC.Collect();//即使垃圾回收
            GC.WaitForPendingFinalizers();
        }

        public void Listen()
        {
            Thread.CurrentThread.IsBackground = true; //后台线程
            try
            {
                while (true)
                {
                    AcceptDone.Reset();
                    read.BeginAccept(new AsyncCallback(AcceptCallback), read);  //异步调用                    
                    AcceptDone.WaitOne();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message + ";" + ex.StackTrace);
                mv.SetConnectionText(1, "ReadCallback error");
            }
        }

        public void SendHeartPackage()
        {
            Thread.CurrentThread.IsBackground = true; //后台线程
            try
            {
                while (true)
                {
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        Socket socket = clientList[i].workSocket;
                        IPEndPoint remotepoint = (IPEndPoint)socket.RemoteEndPoint;
                        try
                        {
                            Send(socket, "12");
                        }
                        catch (Exception ex)
                        {
                            clientList.Remove(clientList[i]);
                            mv.SetConnectionText(0, "IP-" + remotepoint.Address + " Port-" + remotepoint.Port + " stop connect");
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Disconnect(true);
                            socket.Close();
                            LogHelper.Info("Remove IP-" + remotepoint.Address + " Port-" + remotepoint.Port);
                            LogHelper.Error(ex);
                        }
                    }
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message + ";" + ex.StackTrace);
                mv.SetConnectionText(3, "Send heart package error");
            }
        }

        List<StateObject> clientList = new List<StateObject>();
        Dictionary<string, string> dicIPToStation = new Dictionary<string, string>();
        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                AcceptDone.Set();
                Socket temp_socket = (Socket)ar.AsyncState;
                Socket client = temp_socket.EndAccept(ar);
                Control.CheckForIllegalCrossThreadCalls = false;
                IPEndPoint remotepoint = (IPEndPoint)client.RemoteEndPoint;
                string remoteaddr = remotepoint.Address.ToString();
                mv.errorHandler(0, "IP-" + remotepoint.Address + " Port-" + remotepoint.Port + " has connection.", "Success");
                StateObject state = new StateObject();
                state.workSocket = client;
                if (!clientList.Contains(state))
                    clientList.Add(state);
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message + ";" + ex.StackTrace);
                mv.errorHandler(2, ex.Message + ";" + ex.StackTrace, "");
            }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            IPEndPoint remotepoint = (IPEndPoint)handler.RemoteEndPoint;
            // Read data from the client socket. 
            int bytesRead = 0;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.StackTrace);
                return;
            }

            if (bytesRead > 0)
            {
                string contentstr = ""; //接收到的数据
                contentstr += Encoding.GetEncoding("UTF-8").GetString(state.buffer, 0, bytesRead);
                LogHelper.Info("message:" + contentstr);
                string strSN = contentstr.Replace("#", "");

                mv.SetConnectionText(0, "Receive message from (PLC)IP:" + remotepoint.Address + " Port:" + remotepoint.Port + " -----" + strSN);
                string returnValue = "";
                if (strSN.StartsWith("1010"))
                {
                    returnValue = mv.Process1010Command();
                }
                else if (strSN.StartsWith("2010"))
                {

                    returnValue = mv.Process2010Command(strSN);
                }
                else
                {
                    returnValue = "Formate error.";
                }
                byte[] byteData = Encoding.GetEncoding("UTF-8").GetBytes(returnValue);
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
                mv.SetConnectionText(0, "Send message to (PLC)IP:" + remotepoint.Address + " Port:" + remotepoint.Port + " -----" + returnValue + System.Environment.NewLine);
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            else
            {
                mv.SetConnectionText(1, "IP-" + remotepoint.Address + " Port-" + remotepoint.Port + " stop connect");
                handler.Shutdown(SocketShutdown.Both);
                handler.Disconnect(true);
                handler.Close();
                return;
            }
        }

        private void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.GetEncoding("UTF-8").GetBytes(data);
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public static bool IsSocketConnected(Socket s)
        {
            if (s == null)
                return false;
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
        }

        public class StateObject
        {
            // Client  socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }
    }
}
