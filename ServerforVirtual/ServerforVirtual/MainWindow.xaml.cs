using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;


namespace ServerforVirtual
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon MyNotifyIcon;
        public MainWindow()
        {
            this.ResizeMode = ResizeMode.CanMinimize;
            InitializeComponent();
            MyNotifyIcon = new System.Windows.Forms.NotifyIcon();
            string icopath = Environment.CurrentDirectory + "\\1.ico";
            MyNotifyIcon.Icon = new System.Drawing.Icon(icopath); 
            MyNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(MyNotifyIcon_MouseDoubleClick);
        }
        string Mysignature_start = "";
        string Mysignature_end = "";
        void MyNotifyIcon_MouseDoubleClick(object sender,System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void Window_StateChanged_1(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = true;
                MyNotifyIcon.Visible = true;
                this.Hide();
            }
            else if (this.WindowState == WindowState.Normal)
            {
                MyNotifyIcon.Visible = false;
                this.ShowInTaskbar = false;
            }
        }

        private void Listen_Click(object sender, RoutedEventArgs e)
        {
            Utils_.ActionInThread(() =>
            {
                try
                {
                    string ListenPortStr = "";
                    string ListenIPStr = "";
                    Utils_.ActionWithGuiThreadInvoke(Ports, () =>
                    {
                        ListenPortStr = Ports.Text;
                    });
                    Utils_.ActionWithGuiThreadInvoke(IP, () =>
                    {
                        ListenIPStr = IP.Text;
                    });
                    Int32 Port = Convert.ToInt32(ListenPortStr);
                    TcpListener server = null;
                    try
                    {
                        IPAddress LocalAddr = IPAddress.Parse(ListenIPStr);
                        server = new TcpListener(LocalAddr, Port);
                        server.Start();// начинаем ожидание подсоединений клиентов .
                        Utils_.ActionWithGuiThreadInvoke(Comments, () =>
                        {
                            Comments.Text += "Waiting for a connection... ";
                        });
                         while(true)
                         {
                            TcpClient acceptedClient = server.AcceptTcpClient();
                            Utils_.ActionWithGuiThreadInvoke(Comments, () =>
                            {
                                Comments.Text += "\r\n Connected!";
                            });
                              Utils_.ActionInThread(() =>
                               {
                                ByteBuffer bytebuf = new ByteBuffer();
                                byte[] tempBuffer = new byte[4096];
                                TcpClient client = acceptedClient;
                                NetworkStream stream = client.GetStream();
                                int i;
                                while ((i = stream.Read(tempBuffer, 0, tempBuffer.Length)) != 0)
                                {
                                    bytebuf.Append(tempBuffer, i);
                                    Mysignature_start = "<sended_sign>";
                                    Mysignature_end = "</sended_sign>";
                                    if (IsReceiveNetworkDataComplete(bytebuf) == true)
                                    {
                                         string bufStr = "";
                                         string ResStr = "";
                                         bufStr = bytebuf.ToAnsiString(0, bytebuf.Length);
                                         ResStr = Extract.Between(bufStr, Mysignature_start, Mysignature_end);
                                         WorkInMessage(ResStr);
                                    }
                                }
                                client.Close();
                               });
                         }
                    }
                    catch (SocketException err)
                    {
                        Utils_.ActionWithGuiThreadInvoke(Comments, () =>
                        {
                            Comments.Text = "\r\n failed:" + err.ToString();
                        });
                    }
                    finally
                    {
                        server.Stop();
                    }
                }
                catch(Exception exc)
                {
                    Utils_.ActionWithGuiThreadInvoke(Comments , () =>
                    {
                        Comments.Text += "\r\nAdress IP or Port is empty or incorrect..." + exc.ToString();
                    });
                    return;
                }

            });
        }

        public void WorkInMessage(string message)
        {
             if(message == "Shutdown")
             {
                 reboot power = new reboot();
                 power.halt(false, false);//мягкое выключение;
             }
             else if (message == "Reboot")
             {
                 reboot rebt = new reboot();
                 rebt.halt(true, false); //мягкая перезагрузка;
             }
        }

         private bool IsReceiveNetworkDataComplete(ByteBuffer BB)
         {

             bool res = false;
             if (BB.Length > (Mysignature_start.Length + Mysignature_end.Length))
             {
                 string sbend = BB.ToAnsiString(BB.Length - Mysignature_end.Length, Mysignature_end.Length);
                 if (sbend == Mysignature_end)
                 {
                     string sbstart = BB.ToAnsiString(0, Mysignature_start.Length);
                     if (sbstart == Mysignature_start)
                     {
                         res = true;
                     }
                 }
             }
             return res;
         }


    }
}
