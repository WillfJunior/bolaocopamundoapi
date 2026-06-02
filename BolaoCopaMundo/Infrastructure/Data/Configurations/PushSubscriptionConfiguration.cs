using BolaoCopaMundo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolaoCopaMundo.Infrastructure.Data.Configurations;

public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("NEWID()");
        builder.Property(s => s.Endpoint).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.P256dh).IsRequired().HasMaxLength(500);
        builder.Property(s => s.Auth).IsRequired().HasMaxLength(100);
        builder.Property(s => s.DeviceInfo).HasMaxLength(200);
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.HasIndex(s => new { s.UserId, s.Endpoint }).IsUnique();
    }
}
