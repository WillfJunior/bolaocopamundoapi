using BolaoCopaMundo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolaoCopaMundo.Infrastructure.Data.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Phase).IsRequired();
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.MatchDate).IsRequired();
        builder.Property(m => m.ExternalId).HasMaxLength(50);
        builder.Property(m => m.Venue).HasMaxLength(200);
        builder.Property(m => m.MatchLabel).HasMaxLength(200);

        builder.HasMany(m => m.Predictions)
               .WithOne(p => p.Match)
               .HasForeignKey(p => p.MatchId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
