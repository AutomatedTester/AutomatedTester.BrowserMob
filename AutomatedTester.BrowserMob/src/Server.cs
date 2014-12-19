using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;

namespace AutomatedTester.BrowserMob {

    public class Server {

        private const string Host = "localhost";
        private Process _serverProcess;
        private readonly String _path = string.Empty;
        private readonly int _port;

        public Server(string path) : this(path, 8080) {}

        public Server(string path, int port) {
            _path = path;
            _port = port;
        }

        /// <summary>
        /// </summary>
        public string Url {
            get { return String.Format("http://{0}:{1}", Host, _port.ToString(CultureInfo.InvariantCulture)); }
        }

        public void Start() {
            _serverProcess = new Process {
                StartInfo = {
                    FileName = _path
                }
            };
            if (_port != 0) {
                _serverProcess.StartInfo.Arguments = String.Format("--port={0}", _port);
            }

            try {
                _serverProcess.Start();
                int count = 0;
                while (!IsListening()) {
                    Thread.Sleep(1000);
                    count++;
                    if (count == 30) {
                        throw new Exception("Can not connect to BrowserMob Proxy");
                    }
                }
            } catch {
                _serverProcess.Dispose();
                _serverProcess = null;
                throw;
            }
        }

        /// <summary>
        /// </summary>
        public void Stop() {
            if (_serverProcess != null && !_serverProcess.HasExited) {
                _serverProcess.CloseMainWindow();
                _serverProcess.Dispose();
                _serverProcess = null;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public ProxyClient CreateProxy() {
            return new ProxyClient(Host, _port);
        }

        /// <summary>
        /// </summary>
        private bool IsListening() {
            try {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(Host, _port);
                socket.Close();
                return true;
            } catch (Exception) {
                return false;
            }
        }

    }

}