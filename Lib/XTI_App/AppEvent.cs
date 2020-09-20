﻿namespace XTI_App
{
    public sealed class AppEvent
    {
        private readonly AppEventRecord record;

        internal AppEvent(AppEventRecord record)
        {
            this.record = record;
        }

        public int ID { get => record.ID; }
        public string Caption { get => record.Caption; }
        public string Message { get => record.Message; }
        public string Detail { get => record.Detail; }
        public AppEventSeverity Severity() => AppEventSeverity.FromValue(record.Severity);

        public override string ToString() => $"{nameof(AppEvent)} {ID}";
    }
}
