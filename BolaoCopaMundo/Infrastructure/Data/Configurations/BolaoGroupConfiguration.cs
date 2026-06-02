using BolaoCopaMundo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolaoCopaMundo.Infrastructure.Data.Configurations;

public class BolaoGroupConfiguration : IEntityTypeConfiguration<BolaoGroup>
{
    public void Configure(EntityTypeBuilder<BolaoGroup> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasDefaultValueSql("NEWID()");
        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);
        builder.Property(g => g.Description).HasMaxLength(500);
        builder.Property(g => g.InviteCode).IsRequired().HasMaxLength(20);
        builder.Property(g => g.PixKey).HasMaxLength(150);
        builder.HasIndex(g => g.InviteCode).IsUnique();
        builder.Property(g => g.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(g => g.Creator)
               .WithMany()
               .HasForeignKey(g => g.CreatorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(g => g.Members)
               .WithOne(m => m.Group)
               .HasForeignKey(m => m.GroupId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
