using System;

namespace XTI_App
{
    public sealed class AppSessionRecord
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string RequesterKey { get; set; }
        public DateTime TimeStarted { get; set; }
        public DateTime TimeEnded { get; set; }
        public string RemoteAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
