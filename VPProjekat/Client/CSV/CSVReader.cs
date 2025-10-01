using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Client.Csv
{
    public class CsvRow
    {
        public double Volume { get; set; }
        public double T_DHT { get; set; }
        public double T_BMP { get; set; }
        public double Pressure { get; set; }
        public DateTime DateTime { get; set; }
    }

    public static class CsvReader
    {
        public static IEnumerable<CsvRow> ReadFirstN(string path, int n, string rejectsLogPath)
        {
            int count = 0;
            using (var rejects = new StreamWriter(rejectsLogPath, false))
            using (var sr = new StreamReader(path))
            {
                var header = sr.ReadLine();
                if (header == null) yield break;

                var map = TryBindHeaders(header);
                if (map == null)
                {
                    rejects.WriteLine("Neuspešno mapiranje hedera.");
                    yield break;
                }

                while (!sr.EndOfStream && count < n)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    CsvRow row; string reason;
                    if (TryParse(line, map, out row, out reason))
                    {
                        yield return row;
                        count++;
                    }
                    else rejects.WriteLine(reason + " => " + line);
                }
            }
        }

        
        private static Dictionary<string, int> TryBindHeaders(string headerLine)
        {
            var parts = headerLine.Split(',');
            var norm = new string[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                norm[i] = NormalizeHeader(parts[i]);

            
            int iv = IndexOf(norm, new[] { "volume" });
            int idht = IndexOf(norm, new[] { "temperaturedht", "dhttemperature", "dhttemp" });
            int ibmp = IndexOf(norm, new[] { "temperaturebmp", "bmptemperature", "bmptemp" });
            int ip = IndexOf(norm, new[] { "pressure", "bmppressure" });
            int it = IndexOf(norm, new[] { "datetime", "timestamp", "time", "date", "isotime", "datatime" });

            if (iv < 0 || idht < 0 || ibmp < 0 || ip < 0 || it < 0) return null;

            var idx = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            idx["Volume"] = iv;
            idx["T_DHT"] = idht;
            idx["T_BMP"] = ibmp;
            idx["Pressure"] = ip;
            idx["DateTime"] = it;
            return idx;
        }

        
        private static string NormalizeHeader(string raw)
        {
            if (raw == null) return string.Empty;
            string s = raw.Trim();
            s = Regex.Replace(s, @"\[[^\]]*\]", "");
            s = Regex.Replace(s, @"[^A-Za-z0-9]", "");
            s = s.ToLowerInvariant();
            return s;
        }

        private static int IndexOf(string[] haystack, string[] needles)
        {
            for (int i = 0; i < haystack.Length; i++)
            {
                for (int j = 0; j < needles.Length; j++)
                {
                    if (string.Equals(haystack[i], needles[j], StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }
            return -1;
        }

        private static bool TryParse(string line, Dictionary<string, int> m, out CsvRow row, out string reason)
        {
            row = null; reason = null;
            var parts = line.Split(',');
            var ci = CultureInfo.InvariantCulture;

            double vol, tdht, tbmp, pres;
            if (!TryNum(parts, m["Volume"], out vol)) { reason = "Bad Volume"; return false; }
            if (!TryNum(parts, m["T_DHT"], out tdht)) { reason = "Bad T_DHT"; return false; }
            if (!TryNum(parts, m["T_BMP"], out tbmp)) { reason = "Bad T_BMP"; return false; }
            if (!TryNum(parts, m["Pressure"], out pres)) { reason = "Bad Pressure"; return false; }

            var dtRaw = parts[m["DateTime"]];
            DateTime dt;
            if (!DateTime.TryParse(dtRaw, ci, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt))
            {
                if (!DateTime.TryParse(dtRaw, out dt)) { reason = "Bad DateTime"; return false; }
            }

            row = new CsvRow { Volume = vol, T_DHT = tdht, T_BMP = tbmp, Pressure = pres, DateTime = dt };
            return true;
        }

        private static bool TryNum(string[] arr, int i, out double v)
        {
            return double.TryParse(arr[i], NumberStyles.Float, CultureInfo.InvariantCulture, out v);
        }
    }
}
