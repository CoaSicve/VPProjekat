using Common.Contracts;
using System;

namespace Server.Core
{
    public class AnalyticsEngine
    {
        private readonly Thresholds _thr;
        private SensorSample _prev; private bool _hasPrev;
        private long _n; private double _meanV;

        public AnalyticsEngine(Thresholds thr) { _thr = thr; }
        public void Reset() { _prev = null; _hasPrev = false; _n = 0; _meanV = 0; }

        public Tuple<bool, string, bool, string, bool, string, bool, Tuple<string>> Process(SensorSample cur)
        {
            _n++; _meanV = _n == 1 ? cur.Volume : _meanV + (cur.Volume - _meanV) / _n;

            bool v = false, dht = false, bmp = false, oob = false;
            string dv = null, dd = null, db = null, ob = null;

            if (_hasPrev)
            {
                var dV = cur.Volume - _prev.Volume;
                var dDht = cur.T_DHT - _prev.T_DHT;
                var dBmp = cur.T_BMP - _prev.T_BMP;

                if (Math.Abs(dV) > _thr.VThreshold) { v = true; dv = dV >= 0 ? "iznad" : "ispod"; }
                if (Math.Abs(dDht) > _thr.TDhtThreshold) { dht = true; dd = dDht >= 0 ? "iznad" : "ispod"; }
                if (Math.Abs(dBmp) > _thr.TBmpThreshold) { bmp = true; db = dBmp >= 0 ? "iznad" : "ispod"; }
            }

            if (_meanV != 0)
            {
                var low = (1 - _thr.OutOfBandPercent) * _meanV;
                var hi = (1 + _thr.OutOfBandPercent) * _meanV;
                if (cur.Volume < low) { oob = true; ob = "ispod"; }
                if (cur.Volume > hi) { oob = true; ob = "iznad"; }
            }

            _prev = cur; _hasPrev = true;
            return Tuple.Create(v, dv, dht, dd, bmp, db, oob, ob);
        }
    }
}