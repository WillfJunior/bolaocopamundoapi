using BolaoCopaMundo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolaoCopaMundo.Infrastructure.Data.Configurations;

public class BolaoGroupMemberConfiguration : IEntityTypeConfiguration<BolaoGroupMember>
{
    public void Configure(EntityTypeBuilder<BolaoGroupMember> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("NEWID()");
        builder.Property(m => m.Role).IsRequired();
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.InvitedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.HasIndex(m => new { m.GroupId, m.UserId }).IsUnique();

        builder.HasOne(m => m.User)
               .WithMany(u => u.GroupMemberships)
               .HasForeignKey(m => m.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
