using ConnectAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConnectAi.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(t => t.Name)
               .HasMaxLength(128)
               .IsRequired();

        builder.Property(t => t.Slug)
               .HasMaxLength(128)
               .IsRequired();

        builder.Property(t => t.Description)
               .HasMaxLength(512);

        builder.Property(t => t.CreatedAt)
               .HasColumnType("timestamptz")
               .HasDefaultValueSql("now()");

        builder.HasIndex(t => t.Slug)
               .IsUnique();
    }
}
