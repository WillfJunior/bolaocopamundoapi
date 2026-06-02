using BolaoCopaMundo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolaoCopaMundo.Infrastructure.Data.Configurations;

public class PredictionConfiguration : IEntityTypeConfiguration<Prediction>
{
    public void Configure(EntityTypeBuilder<Prediction> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("NEWID()");
        builder.HasIndex(p => new { p.UserId, p.MatchId, p.GroupId }).IsUnique();
        builder.Property(p => p.HomeScore).IsRequired();
        builder.Property(p => p.AwayScore).IsRequired();
        builder.Property(p => p.Points).HasDefaultValue(0);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(p => p.Group)
               .WithMany()
               .HasForeignKey(p => p.GroupId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
