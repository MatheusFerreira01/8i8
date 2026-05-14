using ConnectAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConnectAi.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(m => m.Direction)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(m => m.Content)
               .IsRequired();

        builder.Property(m => m.ExternalMessageId)
               .HasMaxLength(128);

        builder.Property(m => m.SentAt)
               .HasColumnType("timestamptz")
               .IsRequired();

        builder.Property(m => m.CreatedAt)
               .HasColumnType("timestamptz")
               .HasDefaultValueSql("now()");

        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.ExternalMessageId);
    }
}
