using XTI_App;

namespace XTI_WebApp.Fakes
{
    public sealed class FakeHashedPassword : HashedPassword
    {
        public FakeHashedPassword(string password) : base(password)
        {
        }

        protected override string Hash(string password) => $"hashed:{password}";

        protected override bool _Equals(string password, string other) => Value() == other;
    }
}
