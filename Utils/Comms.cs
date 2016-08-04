using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Utils
{
    /// <summary>
    /// An interface to a client.
    /// Hides the nuts and bolts and provides a public interface of just data input and output from a data sender/receiver.
    /// </summary>
    public class Comms
    {
        #region Properties and Fields

        private TcpClient Client { get; set; }
        private MemoryStream ReadStream { get; set; }
        private MemoryStream WriteStream { get; set; }
        private BinaryReader Reader { get; set; }
        private BinaryWriter Writer { get; set; }

        /// <summary>
        /// Useful buffer for reading packeted messages from the server
        /// </summary>
        private byte[] ReadBuffer { get; set; }

        #endregion

        public Comms(TcpClient client)
        {
            Client = client;
            ReadStream = new MemoryStream();
            WriteStream = new MemoryStream();
            Reader = new BinaryReader(ReadStream);
            Writer = new BinaryWriter(WriteStream);

            ReadBuffer = new byte[2048];
            Client.NoDelay = true;

            StartListening();
        }

        #region Data Sending Functions

        /// <summary>
        /// Convert a string to a byte array and then send to our client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="str"></param>
        public void Send(string str)
        {
            SendByteArray(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Send a byte array to our client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bytes"></param>
        protected void SendByteArray(byte[] bytes)
        {
            Writer.Write(bytes);

            int bytesWritten = (int)WriteStream.Position;
            byte[] result = new byte[bytesWritten];

            WriteStream.Position = 0;
            WriteStream.Read(result, 0, bytesWritten);
            WriteStream.Position = 0;

            Client.GetStream().BeginWrite(result, 0, result.Length, null, null);
            Writer.Flush();
        }

        #endregion

        #region Data Receiving Functions

        /// <summary>
        /// Start listening for messages from the server
        /// </summary>
        private void StartListening()
        {
            Client.GetStream().BeginRead(ReadBuffer, 0, 2048, StreamReceived, null);
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
                lock (Client.GetStream())
                {
                    bytesRead = Client.GetStream().EndRead(ar);
                }
            }
            catch { }

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

        #endregion
    }
}
