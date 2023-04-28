using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TcpMessager
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public bool IsClient { get;private set; }
        public string UserName { get; private set; }
        public string Ip { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            IPAddress ip;
            if (IPAddress.TryParse(tb_ip.Text, out ip))
            {
                Ip = tb_ip.Text;
            }
            else
            {
                MessageBox.Show("error 101", "Ip address is incorrect", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(tb_name.Text.Length==0||tb_name.Text.Contains("|"))
            {
                MessageBox.Show("error 203", "user name is incorrect", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            UserName = tb_name.Text;
            DialogResult = true;
            IsClient = true;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (tb_name.Text.Length == 0 || tb_name.Text.Contains("|"))
            {
                MessageBox.Show("error 203", "user name is incorrect", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            UserName = tb_name.Text;
            DialogResult = true;
            IsClient = false;
            Close();
        }
    }
}
