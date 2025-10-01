using System;
using System.Globalization;
using System.IO;
using Common.Contracts;

namespace Server.Core
{
    public class FileStorage : IDisposable
    {
        private readonly string _path;
        private readonly DisposableStreamWriter _meas;
        private readonly DisposableStreamWriter _rej;

        public FileStorage(string root, string sessionTag)
        {
            _path = System.IO.Path.Combine(root, sessionTag);
            System.IO.Directory.CreateDirectory(_path);
            _meas = new DisposableStreamWriter(System.IO.Path.Combine(_path, "measurements_session.csv"), false);
            _rej = new DisposableStreamWriter(System.IO.Path.Combine(_path, "rejects.csv"),false);
            _meas.WriteLine("Volume,T_DHT,T_BMP,Pressure,DateTime");
            _rej.WriteLine("Reason,Volume,T_DHT,T_BMP,Pressure,DateTime");
            
        }

        public void Append(SensorSample s)
        {
            var ci = CultureInfo.InvariantCulture;
            _meas.WriteLine(s.Volume.ToString(ci) + "," + s.T_DHT.ToString(ci) + "," + s.T_BMP.ToString(ci) + "," + s.Pressure.ToString(ci) + "," + s.DateTime.ToString("o", ci));
        }

        public void Reject(string reason, SensorSample s) 
        {
            var ci = CultureInfo.InvariantCulture;
            _rej.WriteLine(reason + "," + (s == null ? "" : s.Volume.ToString(ci)) + "," + (s == null ? "" : s.T_DHT.ToString(ci)) + "," + (s == null ? "" : s.T_BMP.ToString(ci)) + "," + (s == null ? "" : s.Pressure.ToString(ci)) + "," + (s == null ? "" : s.DateTime.ToString("o", ci)));
        }

        public void Dispose() { _meas?.Dispose(); _rej?.Dispose(); }
    }
}
