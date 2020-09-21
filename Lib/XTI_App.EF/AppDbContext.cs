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
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUserRecord>(b =>
            {
                b.HasKey(u => u.ID);
                b.Property(u => u.ID).ValueGeneratedOnAdd();
                b.HasIndex(u => u.UserName).IsUnique();
                b.Property(u => u.UserName).HasMaxLength(100);
                b.Property(u => u.Password).HasMaxLength(100);
            });
            modelBuilder.Entity<AppSessionRecord>(b =>
            {
                b.HasKey(s => s.ID);
                b.Property(s => s.ID).ValueGeneratedOnAdd();
                b.Property(s => s.RequesterKey).HasMaxLength(100);
                b.Property(s => s.RemoteAddress).HasMaxLength(20);
                b.Property(s => s.UserAgent).HasMaxLength(1000);
                b
                    .HasOne<AppUserRecord>()
                    .WithMany()
                    .HasForeignKey(s => s.UserID);
            });
            modelBuilder.Entity<AppRequestRecord>(b =>
            {
                b.HasKey(r => r.ID);
                b.Property(r => r.ID).ValueGeneratedOnAdd();
                b.Property(s => s.Path).HasMaxLength(100);
                b
                    .HasOne<AppSessionRecord>()
                    .WithMany()
                    .HasForeignKey(s => s.SessionID);
                b
                    .HasOne<AppVersionRecord>()
                    .WithMany()
                    .HasForeignKey(s => s.VersionID);
            });
            modelBuilder.Entity<AppEventRecord>(b =>
            {
                b.HasKey(e => e.ID);
                b.Property(e => e.ID).ValueGeneratedOnAdd();
                b.Property(e => e.Caption).HasMaxLength(1000);
                b.Property(e => e.Message).HasMaxLength(5000);
                b.Property(e => e.Detail).HasMaxLength(32000);
                b
                    .HasOne<AppRequestRecord>()
                    .WithMany()
                    .HasForeignKey(e => e.RequestID);
            });
            modelBuilder.Entity<AppRecord>(b =>
            {
                b.HasKey(a => a.ID);
                b.Property(a => a.ID).ValueGeneratedOnAdd();
                b.Property(a => a.Key).HasMaxLength(50);
            });
            modelBuilder.Entity<AppVersionRecord>(b =>
            {
                b.HasKey(v => v.ID);
                b.Property(v => v.ID).ValueGeneratedOnAdd();
                b
                    .HasOne<AppRecord>()
                    .WithMany()
                    .HasForeignKey(v => v.AppID);
            });
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<AppUserRecord> Users { get; }
        public DbSet<AppSessionRecord> Sessions { get; }
        public DbSet<AppRequestRecord> Requests { get; }
        public DbSet<AppEventRecord> Events { get; }
        public DbSet<AppRecord> Apps { get; }
        public DbSet<AppVersionRecord> Versions { get; }
    }
}
