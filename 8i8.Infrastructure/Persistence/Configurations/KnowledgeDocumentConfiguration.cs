using ConnectAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConnectAi.Infrastructure.Persistence.Configurations;

public class KnowledgeDocumentConfiguration : IEntityTypeConfiguration<KnowledgeDocument>
{
    public void Configure(EntityTypeBuilder<KnowledgeDocument> builder)
    {
        builder.ToTable("knowledge_documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(d => d.Title)
               .HasMaxLength(256)
               .IsRequired();

        builder.Property(d => d.Content)
               .IsRequired();

        builder.Property(d => d.Source)
               .HasMaxLength(512);

        builder.Property(d => d.IsActive)
               .HasDefaultValue(true);

        builder.Property(d => d.CreatedAt)
               .HasColumnType("timestamptz")
               .HasDefaultValueSql("now()");

        builder.Property(d => d.UpdatedAt)
               .HasColumnType("timestamptz")
               .HasDefaultValueSql("now()");

        builder.HasMany(d => d.Tags)
               .WithOne(t => t.KnowledgeDocument)
               .HasForeignKey(t => t.KnowledgeDocumentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Embeddings)
               .WithOne(e => e.KnowledgeDocument)
               .HasForeignKey(e => e.KnowledgeDocumentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
