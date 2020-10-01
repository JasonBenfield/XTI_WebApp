using Microsoft.EntityFrameworkCore;

namespace XTI_App.EF
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        {
            Users = Set<AppUserRecord>();
            Sessions = Set<AppSessionRecord>();
            Requests = Set<AppRequestRecord>();
            Events = Set<AppEventRecord>();
            Apps = Set<AppRecord>();
            Versions = Set<AppVersionRecord>();
            Roles = Set<AppRoleRecord>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AppUserEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AppSessionEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AppRequestEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AppEventEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AppEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AppVersionEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AppRoleEntityConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<AppUserRecord> Users { get; }
        public DbSet<AppSessionRecord> Sessions { get; }
        public DbSet<AppRequestRecord> Requests { get; }
        public DbSet<AppEventRecord> Events { get; }
        public DbSet<AppRecord> Apps { get; }
        public DbSet<AppVersionRecord> Versions { get; }
        public DbSet<AppRoleRecord> Roles { get; }
    }
}
