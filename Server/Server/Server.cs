using System;
using System.Net;
using System.Net.Sockets;
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
        /// Our interface to the single client we are supporting for now
        /// </summary>
        public Comms ClientComms { get; private set; }

        /// <summary>
        /// Determines whether we have clients connected
        /// </summary>
        public bool Connections { get; private set; }

        #endregion

        public Server()
        {
            Listener = new TcpListener(IPAddress.Any, 1490);
            Listener.Start();

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
            ClientComms = new Comms(Listener.EndAcceptTcpClient(asyncResult));
            Console.WriteLine("Client accepted");

            ListenForNewClient();
        }
    }
}
