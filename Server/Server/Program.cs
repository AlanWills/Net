using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();

            while (true)
            {
                server.ClientComms.Send(Console.ReadLine());
            }
        }
    }
}