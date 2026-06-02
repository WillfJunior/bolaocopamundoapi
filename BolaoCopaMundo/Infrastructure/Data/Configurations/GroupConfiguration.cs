using BolaoCopaMundo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolaoCopaMundo.Infrastructure.Data.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(5);
        builder.HasIndex(g => g.Name).IsUnique();

        builder.HasMany(g => g.Teams)
               .WithOne(t => t.Group)
               .HasForeignKey(t => t.GroupId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(g => g.Matches)
               .WithOne(m => m.Group)
               .HasForeignKey(m => m.GroupId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
