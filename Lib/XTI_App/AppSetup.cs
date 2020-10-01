﻿using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppSetup
    {
        private readonly AppFactory factory;

        public AppSetup(AppFactory factory)
        {
            this.factory = factory;
        }

        public async Task Run()
        {
            var userRepo = factory.UserRepository();
            var anonUser = await userRepo.User(AppUserName.Anon);
            if (anonUser.IsUnknown())
            {
                await userRepo.Add
                (
                    AppUserName.Anon,
                    new AnonHashedPassword(),
                    new UtcClock().Now()
                );
            }
        }

        private class AnonHashedPassword : IHashedPassword
        {
            public bool Equals(string other) => false;

            public string Value() => "ANON";
        }
    }
}
