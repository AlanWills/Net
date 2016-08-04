using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 0;
            Server server = new Server();

            while (true)
            {
                if (server.Connections)
                {
                    server.SendData(i.ToString());
                    Thread.Sleep(1000);
                    i++;
                }
            }
        }
    }
}