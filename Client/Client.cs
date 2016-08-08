using System;
using System.Net.Sockets;
using System.Text;
using Utils;

namespace Client
{
    public class Client
    {
        #region Properties and Fields
        
        /// <summary>
        /// The interface to the server
        /// </summary>
        public Comms ServerComms { get; private set; }

        #endregion

        public Client()
        {
            // Attempt to connect and quit if the server is not running
            try
            {
                ServerComms = new Comms(new TcpClient("192.168.0.10", 1490));
                ServerComms.OnDataReceived += OnMessageReceived;
                ServerComms.OnDisconnect += OnServerDisconnect;
            }
            catch
            {
                Environment.Exit(0);
            }
        }

        #region Callbacks

        /// <summary>
        /// Simple pass through for now that just prints out all messages send to the client
        /// </summary>
        /// <param name="data"></param>
        private void OnMessageReceived(byte[] data)
        {
            Console.WriteLine(Encoding.UTF8.GetString(data));
        }

        private void OnServerDisconnect()
        {
            Environment.Exit(0);
        }

        #endregion
    }
}
