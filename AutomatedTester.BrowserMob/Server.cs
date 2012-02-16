using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AutomatedTester.BrowserMob
{
    public class Server
    {
        private Process server;
        private int port;
        private String path = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public Server(String path):
            this(path, 8080)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="port"></param>
        public Server(String path, int port)
        {
            this.path = path;
            this.port = port;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            Process builder = new Process();
            builder.StartInfo.FileName = path;
            if (port != 0)
            {
                builder.StartInfo.Arguments = String.Format("--port={0}", port.ToString());
            }
            server = builder;
            try
            {
                server.Start();
                int count = 0;
                while (!IsListening)
                {
                    Thread.Sleep(1000);
                    count++;
                    if (count == 30)
                    {
                        throw new Exception("Can not connect to BrowserMob Proxy");
                    }
                }
            }
            finally
            {
                builder.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (!server.HasExited)
            {
                server.Kill();
            }
            server.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Client CreateProxy(){
            return new Client(Url);
        }

        /// <summary>
        /// 
        /// </summary>
        public String Url
        {
            get { return String.Format("http://localhost:{0}", port.ToString(CultureInfo.InvariantCulture)); }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsListening
        {
            get {
                try
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(IPAddress.Parse("127.0.0.1"), port);
                    socket.Close();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
