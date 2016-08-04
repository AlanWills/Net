using System;
using System.Net.Sockets;
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
            }
            catch
            {
                Environment.Exit(0);
            }
        }        
    }
}
