using System;
using System.ServiceModel;
using Server.Service;

namespace Server
{
    internal static class Program
    {
        static void Main()
        {
            var service = 0;
            using (var host = new ServiceHost(service))
            {
                host.Open();
                Console.WriteLine("WCF Server je pokrenut. Za prekid pritisnite enter.");
                Console.ReadLine();
                host.Close();
            }
        }
    }
}
