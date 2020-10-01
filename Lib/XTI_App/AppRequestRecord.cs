﻿using System;

namespace XTI_App
{
    public sealed class AppRequestRecord
    {
        public int ID { get; set; }
        public int SessionID { get; set; }
        public int VersionID { get; set; }
        public string Path { get; set; }
        public DateTime TimeStarted { get; set; } = Timestamp.MinValue.Value;
        public DateTime TimeEnded { get; set; } = Timestamp.MaxValue.Value;
    }
}
