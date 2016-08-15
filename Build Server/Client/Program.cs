using System;

namespace BuildServerClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();
            
            while (true)
            {
                Console.WriteLine("Ready");
                string message = Console.ReadLine();
                client.ServerComms.Send(message);
            }
        }
    }
}
