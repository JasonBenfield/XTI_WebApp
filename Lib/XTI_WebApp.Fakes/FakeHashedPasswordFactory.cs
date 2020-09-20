using XTI_App;

namespace XTI_WebApp.Fakes
{
    public sealed class FakeHashedPasswordFactory : IHashedPasswordFactory
    {
        public IHashedPassword Create(string password) => new FakeHashedPassword(password);
    }
}
