using System;

namespace Server.Core
{
    public class SessionContext : IDisposable
    {
        public Guid SessionId { get; private set; } = Guid.NewGuid();
        public FileStorage Storage { get; private set; }
        public AnalyticsEngine Analytics { get; private set; }

        public SessionContext(FileStorage st, AnalyticsEngine an) { Storage = st; Analytics = an; }
        public void Dispose() { if (Storage != null) Storage.Dispose(); }
    }
}