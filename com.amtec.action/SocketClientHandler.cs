using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using com.amtec.forms;
using ShippingClient;

namespace com.amtec.action
{
    class SocketClientHandler
    {
        public Socket tcpsend; //发送创建套接字 
        public bool connect_flag = true;
        byte[] buffer = new byte[1024];//设置一个缓冲区，用来保存数据
        public ManualResetEvent connectDone = new ManualResetEvent(false); //连接的信号     
        MainView view = null;

        public SocketClientHandler(MainView _view)
        {
            view = _view;
        }

        public bool connect(string address, string port)
        {
            try
            {
                tcpsend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//初始化套接字
                IPEndPoint remotepoint = new IPEndPoint(IPAddress.Parse(address), Convert.ToInt32(port));//根据ip地址和端口号创建远程终结点
                EndPoint end = (EndPoint)remotepoint;
                view.errorHandler(0, view.Message("msg_Start Server connection.") + "IP:" + remotepoint.Address + " Port:" + remotepoint.Port, "");
                tcpsend.Connect(end);
                view.errorHandler(0, view.Message("msg_Connected to server success."), "");
                view.SetTopWindowMessage(view.Message("msg_Connected to server success."), "");
                //tcpsend.BeginConnect(end, new AsyncCallback(ConnectedCallback), tcpsend); //调用回调函数
                Thread.Sleep(2000);
                tcpsend.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), tcpsend);
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                view.errorHandler(3, view.Message("msg_Connected to server error."), "Connected to server error.");
                return false;
            }
        }

        private void ConnectedCallback(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            try
            {
                client.EndConnect(ar);
                view.errorHandler(0, "Connected to Server.", "");
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message + "," + ex.StackTrace);
                view.errorHandler(2, "Connection Error. IP Cannot be reached.", "");
            }
        }

        public void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                var socket = ar.AsyncState as Socket;
                var length = socket.EndReceive(ar);
                //读取出来消息内容
                var message = Encoding.GetEncoding("UTF-8").GetString(buffer, 0, length);
                view.ProcessPFCMessage(message);
                //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }

        public bool send(string data)
        {
            try
            {
                int length = data.Length;
                Byte[] Bysend = new byte[length];
                Bysend = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(data); //将字符串指定到指定Byte数组
                tcpsend.BeginSend(Bysend, 0, Bysend.Length, 0, new AsyncCallback(SendCallback), tcpsend); //异步发送数据
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                return false;
            }
        }

        private void SendCallback(IAsyncResult ar) //发送的回调函数
        {
            Socket client = (Socket)ar.AsyncState;
            int bytesSend = client.EndSend(ar); //完成发送
        }

        public void CloseSocket()
        {
            tcpsend.Close();
        }

        public string SendData(string message)
        {
            try
            {
                Thread.Sleep(1000);
                send(message);
                //view.errorHandler(0, "send success", "");
                LogHelper.Info("send message:" + message);
                //readDone.Reset();
                byte[] back = new byte[1024 * 1024];
                int count = tcpsend.Receive((back));
                //string str = Encoding.GetEncoding("UTF-8").GetString(back, 0, count);
                string str = Encoding.Default.GetString(back, 0, count);//Encoding.GetEncoding("UTF-8").GetString(back, 0, count);
                //view.errorHandler(0, str, "");
                LogHelper.Info("receive message:" + str);
                return str;
            }
            catch (Exception ex)
            {
                view.errorHandler(2, "send error", "");
                LogHelper.Error(ex.Message + "," + ex.StackTrace);
                return null;
            }
        }
    }
}
