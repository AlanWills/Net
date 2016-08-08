using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();
            
            while (true)
            {
                client.ServerComms.Send(Console.ReadLine());
            }
        }
    }
}
