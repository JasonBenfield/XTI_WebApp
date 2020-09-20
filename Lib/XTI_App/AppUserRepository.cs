using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace XTI_App
{
    public sealed class AppUserRepository
    {
        private readonly AppFactory factory;
        private readonly DataRepository<AppUserRecord> repo;

        public AppUserRepository(AppFactory factory, DataRepository<AppUserRecord> repo)
        {
            this.factory = factory;
            this.repo = repo;
        }

        public async Task<AppUser> RetrieveByUserName(AppUserName userName)
        {
            var userRecord = await repo.Retrieve().FirstOrDefaultAsync(u => u.UserName == userName.Value());
            return factory.CreateAppUser(userRecord ?? new AppUserRecord());
        }

        public async Task<AppUser> Add(AppUserName userName, IHashedPassword password, DateTime timeAdded)
        {
            var newUser = new AppUserRecord
            {
                UserName = userName.Value(),
                Password = password.Value(),
                TimeAdded = timeAdded
            };
            await repo.Create(newUser);
            return factory.CreateAppUser(newUser);
        }
    }
}
