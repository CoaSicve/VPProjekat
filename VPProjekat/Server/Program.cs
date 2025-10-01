using System;
using System.ServiceModel;
using Server.Service;

namespace Server
{
    internal static class Program
    {
        static void Main()
        {
            var baseAddress = new Uri("net.tcp://localhost:9009/OfficeSensor");
            var rootPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

            var service = new KancelarijaSensorService(rootPath);
            service.OnTransferStarted += (s, e) => Console.WriteLine("[" + e.SessionId + "] " + e.Message);
            service.OnSampleReceived += (s, e) => Console.WriteLine("[" + e.SessionId + "] Sample @ " + e.Sample.DateTime.ToString("o"));
            service.OnTransferCompleted += (s, e) => Console.WriteLine("[" + e.SessionId + "] " + e.Message);
            service.OnWarningRaised += (s, e) => Console.WriteLine("[" + e.SessionId + "] WARNING " + e.Kind + " (" + e.Direction + "): " + e.Message);
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
