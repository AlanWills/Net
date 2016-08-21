using System;
using System.Text;
using Utils;

namespace BuildServerClient
{
    public class Client : BaseClient
    {
        public Client() :
            base("192.168.0.30")
        {

        }

        #region Callbacks

        /// <summary>
        /// Simple pass through for now that just prints out all messages send to the client
        /// </summary>
        /// <param name="data"></param>
        protected override void OnMessageReceived(byte[] data)
        {
            Console.WriteLine(Encoding.UTF8.GetString(data));
        }

        protected override void OnServerDisconnect()
        {
            Environment.Exit(0);
        }

        #endregion
    }
}
