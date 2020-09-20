using System;

namespace XTI_App
{
    public sealed class UtcClock : Clock
    {
        public DateTime Now() => DateTime.UtcNow;

        public DateTime Today() => DateTime.UtcNow.Date;
    }
}
