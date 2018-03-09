using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientforVirtual
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Работа с формой

        private void Scan_Serv_Click(object sender, RoutedEventArgs e)
        {
            Scan_Serv.IsEnabled = false;
            CheckActiveMachines();

        }
        private void OnSelectedItemClicked(object sender, RoutedEventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            string menuItemHeader = menu.Header.ToString();
            ServerInformation ritem = (ServerInformation)ListView_Serv.SelectedItem;

            if (menuItemHeader == "Shutdown")
            {
                SendCommandToClient(menuItemHeader, ritem);
            }
            else if (menuItemHeader == "Reboot")
            {
                SendCommandToClient(menuItemHeader, ritem);
            }

        }
        #endregion

        #region Главные методы
        /// <summary>
        /// Проверка включенных компьютеров
        /// </summary>
        public void CheckActiveMachines()
        {
            List<ServerInformation> ServInf = new List<ServerInformation>();
            List<ServerInformation> Viewcoll = new List<ServerInformation>();
            ListView_Serv.ItemsSource = null;
            int counter = 0;
            ServInf = GetServerInformation();
            Utils_.ActionInThread(() =>
            {
                foreach (var elt in ServInf)
                {
                    string IPadr = elt.IPAddres;
                    string Portstr = elt.Port;
                    TcpClient Client = new TcpClient();
                    int Port = Convert.ToInt32(Portstr);
                    try
                    {
                        Client.Connect(IPadr, Port);
                        counter += 1;
                        elt.Position = (counter).ToString();
                        Viewcoll.Add(elt);
                        Utils_.ActionWithGuiThreadInvoke(ListView_Serv, () =>
                        {
                            ListView_Serv.ItemsSource = null;
                            ListView_Serv.ItemsSource = Viewcoll; ;
                        });

                        Client.Close();
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show("Удаленный сервер Offline , подключаюсь к следующему из settings.ini :" + exc.Message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                if (Viewcoll.Count == 0)
                {
                    MessageBox.Show("Нет Online серверов!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                Utils_.ActionWithGuiThreadInvoke(ListView_Serv, () =>
                {
                    Scan_Serv.IsEnabled = true;
                });
            });
        }
        /// <summary>
        /// Отправка команды на удаленный компьютер
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ServIn"></param>
        public void SendCommandToClient(string message, ServerInformation ServIn)
        {
            try
            {
                string IPadr = ServIn.IPAddres;
                string Portstr = ServIn.Port;
                TcpClient Client = new TcpClient();
                int Port = Convert.ToInt32(Portstr);
                try
                {
                    Client.Connect(IPadr, Port);

                    string dat = "<sended_sign>" + message + "</sended_sign>";
                    NetworkStream stream = Client.GetStream();
                    try
                    {
                        Byte[] data = System.Text.Encoding.ASCII.GetBytes(dat);
                        stream.Write(data, 0, data.Length);
                        MessageBox.Show("Удаленный сервер : " + ServIn.Name + " " + ServIn.IPAddres + ":" + ServIn.Port + " " + message + "!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("failed !" + ex.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    stream.Close();
                    Client.Close();
                }
                catch
                {
                    MessageBox.Show("Нет соединения с удаленным сервером!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                MessageBox.Show("Ip адрес или порт некорректны !", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        #region Рабочие методы и классы
        public class ServerInformation
        {
            public string Position { get; set; }
            public string Name { get; set; }
            public string IPAddres { get; set; }
            public string Port { get; set; }
            public string Online { get; set; }
        }

        List<ServerInformation> GetServerInformation()
        {
            List<ServerInformation> resultCollection = new List<ServerInformation>();
            string settingsInform = File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + "settings.ini");
            settingsInform = Extract.Between(settingsInform, "#servers=", ";");
            string[] m_dict = settingsInform.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var el in m_dict)
            {
                resultCollection.Add(GetInfo(el));
            }
            return resultCollection;
        }

        ServerInformation GetInfo(string servInf)
        {
            ServerInformation serverinformation = new ServerInformation();
            serverinformation.Position = "";
            serverinformation.Name = Extract.BetweenStart(servInf, "-");
            serverinformation.IPAddres = Extract.Between(servInf, "-", ":");
            serverinformation.Port = Extract.BetweenEnd(servInf, ":");
            serverinformation.Online = "Yes";
            return serverinformation;
        }
        #endregion


    }
}
