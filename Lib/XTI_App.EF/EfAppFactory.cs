using Microsoft.EntityFrameworkCore;
using System;

namespace XTI_App.EF
{
    public sealed class EfAppFactory : AppFactory
    {
        public EfAppFactory(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        private readonly AppDbContext appDbContext;

        protected override DataRepository<T> CreateDataRepository<T>() where T : class
        {
            var dbSet = getDbSet<T>();
            return new EfDataRepository<T>(appDbContext, dbSet);
        }

        private DbSet<T> getDbSet<T>() where T : class
        {
            object obj;
            if (typeof(T) == typeof(AppSessionRecord))
            {
                obj = appDbContext.Sessions;
            }
            else if (typeof(T) == typeof(AppUserRecord))
            {
                obj = appDbContext.Users;
            }
            else if (typeof(T) == typeof(AppRequestRecord))
            {
                obj = appDbContext.Requests;
            }
            else if (typeof(T) == typeof(AppEventRecord))
            {
                obj = appDbContext.Events;
            }
            else
            {
                throw new ArgumentException($"DbSet not found for type {typeof(T)}");
            }
            var dbSet = (DbSet<T>)obj;
            return dbSet;
        }
    }
}
