using System.Net.Sockets;
using System.Net;
using System.Windows;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Linq;
using System.Net.Http;
using static TcpMessager.MainWindow;
using System.Windows.Controls;
using System.Runtime.InteropServices;

namespace TcpMessager
{

    internal class Server
    {
        public delegate void UserDisconnect(string str);
       public event UserDisconnect OnUserDisconnect;
        public delegate void message(string message);
        public event message OnMessage;
        public string Name{get;set;}
        public void Disconnect(TcpClient client)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Item1 == client)
                {
                    OnUserDisconnect?.Invoke($"{clients[i].Item2} : Disconnect");
                    using (StreamWriter sw = new StreamWriter(client.GetStream()))
                    {
                        sw.AutoFlush = true;
                        sw.WriteLine($"{MessageType.Disconnect}|");
                    }
                    client.Close();
                    clients.RemoveAt(i);
                    return;
                }
            }
        }
        public delegate void Users(List<(string name, TcpClient client)> users);
        public event Users UsersChanged;
        ~Server()
        {
            IsStop = true;
        }
        public bool IsStop;
        public int Port { get; private set; }
        private TcpListener listener;
        private List<(TcpClient, string)> clients = new List<(TcpClient, string)>();
        public Server(int port)
        {
            Port = port;
            listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            new Thread(new ThreadStart(listening)).Start();
            new Thread(new ThreadStart(SendUsers)).Start();


        }
        private void listening()
        {
            while (true)
            {
                    var client = listener.AcceptTcpClient();
                Task.Factory.StartNew(() =>
                {
                    var sr = new StreamReader(client.GetStream());
                    while (client.Connected)
                    {
                        try
                        {
                            var line = sr.ReadLine();
                            string[] msg = line.Split('|');

                            switch (msg[0])
                            {
                                case "Connect": clients.Add((client, msg[1])); break;
                                case "Disconnect":
                                    for (int i = 0; i < clients.Count; i++)
                                    {
                                        if (clients[i].Item1.Client == client.Client)
                                        {
                                            using (StreamWriter sw = new StreamWriter(client.GetStream()))
                                            {
                                                sw.AutoFlush = true;
                                                sw.WriteLine($"{MessageType.Disconnect}|");
                                            }
                                            OnUserDisconnect?.Invoke($"{clients[i].Item2} : Disconnect");
                                            clients.RemoveAt(i);
                                            break;
                                        }
                                    }
                                    break;
                                case "Message":
                                    string s = $"{clients.FirstOrDefault(x => x.Item1 == client).Item2}:{msg[1]}";
                                    SendToAll(s);
                                    OnMessage(s);
                                    break;
                            }
                        }
                        catch
                        {
                            client.Dispose();
                        }
                    }
                });
            }
        }

        public async void SendToAll(string s)
        {
            await Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    try
                    {
                        var sw = new StreamWriter(clients[i].Item1.GetStream());
                        sw.AutoFlush = true;
                        sw.WriteLine($"{MessageType.Message}|{s}");
                    }
                    catch
                    {

                    }

                }
            });
        }

        async void SendUsers()
        {
            await Task.Factory.StartNew(() =>
            {
                while (true)
                {                  
                    Thread.Sleep(1000);
                    List<(string name, TcpClient client)> users = new List<(string name, TcpClient client)>();
                    foreach (var item in clients)
                        users.Add((item.Item2,item.Item1));
                    UsersChanged(users);


                    List<string> l = new List<string>();
                    foreach (var item in users)
                        l.Add(item.Item1);
                    l.Add(Name);

                    string str = string.Join("|", l);
                    for (int i = 0; i < clients.Count; i++)
                    {
                        try
                        {
                            var sw = new StreamWriter(clients[i].Item1.GetStream());
                            sw.AutoFlush = true;
                            sw.WriteLine($"{MessageType.GetUsers}|{str}");                           

                        }
                        catch
                        {
                            clients.RemoveAt(i);
                            OnUserDisconnect?.Invoke($"{clients[i].Item2} : Disconnect");
                        }

                    }
                }
            });
        }

    }
}