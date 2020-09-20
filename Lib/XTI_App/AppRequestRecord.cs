using System;

namespace XTI_App
{
    public sealed class AppRequestRecord
    {
        public int ID { get; set; }
        public int SessionID { get; set; }
        public string ResourceName { get; set; }
        public DateTime TimeRequested { get; set; }
    }
}
