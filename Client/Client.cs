using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Client
    {
        #region Properties and Fields
        
        /// <summary>
        /// The actual interface to the server
        /// </summary>
        private TcpClient ClientImpl { get; set; }

        /// <summary>
        /// Useful buffer for reading packeted messages from the server
        /// </summary>
        private byte[] ReadBuffer { get; set; }

        #endregion

        public Client()
        {
            // Attempt to connect and quit if the server is not running
            try
            {
                ClientImpl = new TcpClient("127.0.0.1", 1490);
            }
            catch
            {
                Environment.Exit(0);
            }

            ReadBuffer = new byte[2048];
            ClientImpl.NoDelay = true;

            StartListening();
        }

        /// <summary>
        /// Start listening for messages from the server
        /// </summary>
        private void StartListening()
        {
            ClientImpl.GetStream().BeginRead(ReadBuffer, 0, 2048, StreamReceived, null);
        }

        /// <summary>
        /// Callback which processes a message sent from the server
        /// </summary>
        /// <param name="ar"></param>
        private void StreamReceived(IAsyncResult ar)
        {
            int bytesRead = 0;
            try
            {
                lock (ClientImpl.GetStream())
                {
                    bytesRead = ClientImpl.GetStream().EndRead(ar);
                }
            }
            catch (Exception e) { }

            //Create the byte array with the number of bytes read
            byte[] data = new byte[bytesRead];

            //Populate the array
            for (int i = 0; i < bytesRead; i++)
            {
                data[i] = ReadBuffer[i];
            }

            string text = Encoding.UTF8.GetString(data);
            text = text.Trim();
            Console.WriteLine(text);

            //Listen for new data
            StartListening();
        }
    }
}
