namespace XTI_App
{
    public sealed class AppUser
    {
        internal AppUser(AppUserRecord record)
        {
            this.record = record ?? new AppUserRecord();
        }

        private readonly AppUserRecord record;

        public int ID { get => record.ID; }
        public AppUserName UserName() => new AppUserName(record.UserName);
        public bool IsUnknown() => UserName().Equals("");

        public bool IsPasswordCorrect(IHashedPassword hashedPassword) => hashedPassword.Equals(record.Password);

        public override string ToString() => $"{nameof(AppUser)} {ID}";

    }
}
