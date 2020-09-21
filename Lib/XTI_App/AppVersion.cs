namespace XTI_App
{
    public sealed class AppVersion
    {
        private readonly AppVersionRecord record;

        internal AppVersion(AppVersionRecord record)
        {
            this.record = record ?? new AppVersionRecord();
        }

        public int ID { get => record.ID; }

        public override string ToString() => $"{nameof(AppVersion)} {ID}";
    }
}
