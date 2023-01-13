using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AForge.Video;

namespace RemoteServer
{
    public partial class Server : Form
    {
        int count = 0;
        private int PORT = 1234;
        private Socket _serverSocket;
        private Socket acc;
        private NetworkStream netStream;
        private BinaryReader sreader;
        private BinaryWriter swriter;
        private ScreenCaptureStream _screenCapture;
        public Server()
        {
            InitializeComponent();
            this.Opacity = 0; // I do not want the server to see the form

            //intiallizing the screenCapture neccesery props
            _screenCapture = new ScreenCaptureStream(new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), 100);
            _screenCapture.NewFrame += new NewFrameEventHandler(NewFrame);
        }

        public byte[] ImageToByte(Image img)
        {
            ImageConverter conv = new ImageConverter();
            return (byte[])conv.ConvertTo(img, typeof(byte[]));
        }

        private void Form1_Shown(object sender, EventArgs e)
        {//Hide the form completely (not just the opacity)
            this.Hide();
            this.Opacity = 100;
            try
            {
                startServer();
            }
            catch (Exception ex)
            { return; }
            /*finally
            {
                acc.Close();
                _serverSocket.Close();
                _screenCapture.Stop();
                this.Close();
            }*/
            /*new System.Threading.Thread(delegate ()
            {
                while (true)
                {
                    pictureBox1.Image = GetImage();
                    //System.Threading.Thread.Sleep(100);
                }
            }).Start();*/
        }

        private void startServer()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(0, PORT));
            _serverSocket.Listen(1);

            acc = _serverSocket.Accept();


            netStream = new NetworkStream(acc);
            sreader = new BinaryReader(netStream);

            if (sreader.ReadString() == "startShare")
                _screenCapture.Start();
            else
                throw new Exception("Did not start share");
        }

        public void NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            byte[] data;

            try
            {

                Image currentCapture = (Image)eventArgs.Frame;

                data = ImageToByte(currentCapture);
                count++;
                Console.WriteLine(count);
                swriter = new BinaryWriter(netStream);
                swriter.Write(data.Length.ToString());
                Console.WriteLine();
                Console.WriteLine(DateTime.Now.ToString() + "STRING");
                acc.Send(data, 0, data.Length, 0);
                Console.WriteLine(DateTime.Now.ToString() + "IMAGE");
                swriter.Flush();
                System.Threading.Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                _serverSocket.Close();
                _screenCapture.Stop();
                MessageBox.Show("Closing Application");
                this.Close();
                return;
            }
        }
    }
}

