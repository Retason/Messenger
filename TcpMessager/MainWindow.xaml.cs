using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TcpMessager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum MessageType
        {
            Connect,
            Disconnect,
            Message,
            GetUsers
        }
        Server server;
        Client client;
        private string name;
        public MainWindow()
        {
            LoginWindow loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog().Value)
            {
                InitializeComponent();
                name = loginWindow.UserName;
                if (loginWindow.IsClient)
                {
                    client = new Client();
                    client.Name = loginWindow.UserName;
                    client.Ip = loginWindow.Ip;
                    client.OnMessage += (e) => Dispatcher.Invoke(new Action(() => { tb.Text += e; }));
                    client.OnUsers += Client_OnUsers;
                    client.OnDisconnect += Client_Disconnect;
                    client.Connect();
                }
                else
                {
                    server = new Server(8888);
                    server.Name = loginWindow.UserName;    
                    server.OnMessage += (e) => Dispatcher.Invoke(new Action(() => { tb.Text += $"{e}\n"; }));
                    server.OnUserDisconnect += (e) => Dispatcher.Invoke(new Action(() => { tb.Text += $"{e}\n"; }));
                    server.UsersChanged += Server_UsersChanged;
                }

                    Title += $" : {loginWindow.UserName}";
            }
            else
            {
                Close();
            }
        }

        private void Server_UsersChanged(List<(string name, TcpClient client)> users)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                lb.Items.Clear();
                foreach (var item in users)
                {
                    ListBoxItem lbi = new ListBoxItem()
                    {
                        Content = item.name,
                    };

                    var mi = new MenuItem()
                    {
                        DataContext = item.client,
                        Header = "Disconnect",
                    };
                    mi.Click += Mi_Click;

                    var con = new ContextMenu();
                    con.Items.Add(mi);

                    lbi.ContextMenu = con;
                    lb.Items.Add(lbi);
                }
            }));
        }

        private void Mi_Click(object sender, RoutedEventArgs e)
        {
            server.Disconnect((sender as MenuItem).DataContext as TcpClient);
        }

        private void Client_Disconnect()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                MessageBox.Show("Disconnect", "Admin kick you", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }));
        }
        private void Client_OnUsers(string[] users)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                lb.Items.Clear();
                foreach (var item in users)
                    lb.Items.Add(item);

            }));
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (tbm.Text.Length == 0)
                return;

            if (tbm.Text == "\\Disconnect")
            {
                Close();
            }
            else
            {
                if (client != null)
                {
                    client.Send(tbm.Text);
                }
                else
                {
                    string msg = $"{name} : {tbm.Text}";
                    server.SendToAll(msg);
                    tb.Text += $"{msg}\n";
                }
                tbm.Focus();
                tbm.Text = "";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
