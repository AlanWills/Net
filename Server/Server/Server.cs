using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Utils;

namespace Server
{
    public class Server
    {
        #region Properties and Fields

        /// <summary>
        /// The listener we can use to detect incoming connections from clients to the server
        /// </summary>
        private TcpListener Listener { get; set; }

        /// <summary>
        /// A server side client interface we can use to send data to a remote
        /// </summary>
        private TcpClient Client { get; set; }

        /// <summary>
        /// Communications interface
        /// </summary>
        public Comms Comms { get; set; }

        /// <summary>
        /// Determines whether we have clients connected
        /// </summary>
        public bool Connections { get; private set; }

        #endregion

        public Server()
        {
            Listener = new TcpListener(IPAddress.Any, 1490);
            Listener.Start();

            Comms = new Comms();

            ListenForNewClient();
        }

        /// <summary>
        /// Starts an asynchronous check for new connections
        /// </summary>
        private void ListenForNewClient()
        {
            Listener.BeginAcceptTcpClient(AcceptClient, null);
        }

        /// <summary>
        /// Callback for when a new client connects to the server
        /// </summary>
        /// <param name="asyncResult"></param>
        private void AcceptClient(IAsyncResult asyncResult)
        {
            Client = Listener.EndAcceptTcpClient(asyncResult);
            Connections = true;

            ListenForNewClient();
        }
    }
}
