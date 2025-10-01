using System.Configuration;
using System.Globalization;

namespace Server.Core
{
    public class Thresholds
    {
        public double VThreshold { get; private set; }
        public double TDhtThreshold { get; private set; }
        public double TBmpThreshold { get; private set; }
        public double OutOfBandPercent { get; private set; }

        public Thresholds()
        {
            VThreshold = Read("V_threshold", 5.0);
            TDhtThreshold = Read("T_dht_threshold", 1.0);
            TBmpThreshold = Read("T_bmp_threshold", 1.0);
            OutOfBandPercent = Read("OutOfBandPercent", 0.25);
        }

        private static double Read(string key, double def)
        {
            var raw = ConfigurationManager.AppSettings[key];
            double v;
            return double.TryParse(raw, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out v) ? v : def;
        }
    }
}