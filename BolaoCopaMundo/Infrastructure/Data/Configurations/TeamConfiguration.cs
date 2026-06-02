using BolaoCopaMundo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolaoCopaMundo.Infrastructure.Data.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.FifaCode).IsRequired().HasMaxLength(5);
        builder.HasIndex(t => t.FifaCode).IsUnique();
        builder.Property(t => t.FlagUrl).HasMaxLength(500);

        builder.HasMany(t => t.HomeMatches)
               .WithOne(m => m.HomeTeam)
               .HasForeignKey(m => m.HomeTeamId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.AwayMatches)
               .WithOne(m => m.AwayTeam)
               .HasForeignKey(m => m.AwayTeamId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
