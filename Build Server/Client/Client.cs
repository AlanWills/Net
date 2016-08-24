using System;
using Utils;

namespace BuildServerClient
{
    public class Client : BaseClient
    {
        public Client() :
            base("127.0.0.1")
        {

        }

        #region Callbacks

        /// <summary>
        /// Simple pass through for now that just prints out all messages send to the client
        /// </summary>
        /// <param name="data"></param>
        protected override void OnMessageReceived(byte[] data)
        {
            Console.WriteLine(data.ConvertToString());
        }

        protected override void OnServerDisconnect()
        {
            Environment.Exit(0);
        }

        #endregion
    }
}
