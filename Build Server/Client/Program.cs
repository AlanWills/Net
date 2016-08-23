using System;

namespace BuildServerClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Build Server Client";
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
