using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml.Serialization;
using static TcpMessager.MainWindow;

namespace TcpMessager
{
    internal class Client
    {

        public delegate void Users(string[] users);
        public event Users OnUsers;
        public Action OnDisconnect;
        public delegate void message(string message);
        public event message OnMessage;
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 8888;
        public string Name { get; set; }

        TcpClient client;
        StreamReader sr;
        StreamWriter sw;

        public Client()
        {
            Task.Factory.StartNew(() =>
            { Listener(); });
        }
        private void Listener ()
        {
            while (true)
            {
                try
                {
                    if (client?.Connected == true)
                    {
                        byte[] data = new byte[512];
                        var stream = client.GetStream();
                        int bytes = stream.ReadAsync(data, 0, 512).Result;

                        string[] str = Encoding.UTF8.GetString(data, 0, bytes).Split('|');
                        switch (str[0])
                        {
                            case "Disconnect": OnDisconnect?.Invoke(); return;
                            case "Message": OnMessage(str[1]); break;
                            case "GetUsers":
                                List<string> list = new List<string>();
                                for (int i = 1; i < str.Length; i++)
                                {
                                    list.Add(str[i]);
                                }
                                OnUsers?.Invoke(list.ToArray());
                                ; break;
                        }
                        continue;
                    }                   
                    Task.Delay(10).Wait();
                }
                catch 
                {

                }
            }
        }
        public void Connect()
        {
            client = new TcpClient();
            client.Connect(Ip, Port);
            sw = new StreamWriter(client.GetStream());
            sr = new StreamReader(client.GetStream());
            sw.AutoFlush = true;
            sw.WriteLine($"{MessageType.Connect}|{Name}");
        }
        public void Send(string message)
        {
                try
                {
                    sw.WriteLine($"{MessageType.Message}|{message}");
                }
                catch 
                { }
        }


    }
}
