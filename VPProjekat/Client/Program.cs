using System;
using System.IO;
using System.ServiceModel;
using Common.Contracts;
using Client.Csv;


namespace Client
{
    internal static class Program
    {
        static void Main()
        {
            Console.WriteLine("Start klijentske strane. ");

            string csvPath = @"C:\Users\Aco\source\repos\CoaSicve\VPProjekat\VPProjekat\AirPi Data - AirPi.csv";
            string rejectsLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_rejects.log");

            var binding = new NetTcpBinding("tcpStream"); binding.Security.Mode = SecurityMode.None;
            var endpoint = new EndpointAddress("net.tcp://localhost:9009/OfficeSensor");
            var factory = new ChannelFactory<IKancelarijaSensorService>(binding, endpoint);
            var proxy = factory.CreateChannel();

            using (var enumRows = CsvReader.ReadFirstN(csvPath, 100, rejectsLog).GetEnumerator())
            {
                if (!enumRows.MoveNext())
                {
                    Console.WriteLine("Nema validnih redova u CSV.");
                    return;
                }
                var first = enumRows.Current;

                var start = proxy.StartSession(new MetaHeader
                {
                    Volume = first.Volume,
                    T_DHT = first.T_DHT,
                    T_BMP = first.T_BMP,
                    Pressure = first.Pressure,
                    DateTime = first.DateTime
                });
                Console.WriteLine("StartSession => " + start.Ack + " " + start.Status + " " + start.Message);

                int sent = 0;
                while (enumRows.MoveNext())
                {
                    var r = enumRows.Current;
                    var s = new SensorSample
                    {
                        Volume = r.Volume,
                        T_DHT = r.T_DHT,
                        T_BMP = r.T_BMP,
                        Pressure = r.Pressure,
                        DateTime = r.DateTime
                    };
                    try
                    {
                        var ack = proxy.PushSample(s);
                        Console.WriteLine("PushSample(" + s.DateTime.ToString("o") + ") => " + ack.Ack + " " + ack.Status + " " + ack.Message);
                    }
                    catch (FaultException fe)
                    {
                        Console.WriteLine("Fault: " + fe.Message);
                    }
                    sent++;
                    System.Threading.Thread.Sleep(50);
                }

                var end = proxy.EndSession();
                Console.WriteLine("EndSession => " + end.Ack + " " + end.Status + " " + end.Message);
            }

            ((IClientChannel)proxy).Close();
            factory.Close();
        }
    }
}
