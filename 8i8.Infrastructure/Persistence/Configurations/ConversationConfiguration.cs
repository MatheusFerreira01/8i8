using ConnectAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConnectAi.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.PhoneNumber)
               .HasMaxLength(32)
               .IsRequired();

        builder.Property(c => c.ContactName)
               .HasMaxLength(128);

        builder.Property(c => c.InstanceName)
               .HasMaxLength(128);

        builder.Property(c => c.CreatedAt)
               .HasColumnType("timestamptz")
               .HasDefaultValueSql("now()");

        builder.Property(c => c.UpdatedAt)
               .HasColumnType("timestamptz")
               .HasDefaultValueSql("now()");

        // Conversa única por telefone + instância (não cria conversa duplicada
        // se o mesmo número escrever pela mesma instância). Útil pra Upsert.
        builder.HasIndex(c => new { c.PhoneNumber, c.InstanceName })
               .IsUnique();

        builder.HasMany(c => c.Messages)
               .WithOne(m => m.Conversation)
               .HasForeignKey(m => m.ConversationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
