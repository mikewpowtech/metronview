using System;

namespace Utilities
{
    public abstract record Reading
    {
        public DateTime DateReceivedUtc { get; init; }
        public DateTime DateRecordedUtc { get; init; }
        public int Channel { get; init; }
        public bool IsAlarm { get; init; }

        public abstract void Save(IReadingsDb db);
    }
}