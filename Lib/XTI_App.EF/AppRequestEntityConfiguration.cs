using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace XTI_App.EF
{
    public sealed class AppRequestEntityConfiguration : IEntityTypeConfiguration<AppRequestRecord>
    {
        public void Configure(EntityTypeBuilder<AppRequestRecord> builder)
        {
            builder.HasKey(r => r.ID);
            builder.Property(r => r.ID).ValueGeneratedOnAdd();
            builder.Property(s => s.Path).HasMaxLength(100);
            builder
                .HasOne<AppSessionRecord>()
                .WithMany()
                .HasForeignKey(s => s.SessionID);
            builder
                .HasOne<AppVersionRecord>()
                .WithMany()
                .HasForeignKey(s => s.VersionID);
        }
    }
}
