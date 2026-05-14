using ConnectAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConnectAi.Infrastructure.Persistence.Configurations;

public class KnowledgeDocumentTagConfiguration : IEntityTypeConfiguration<KnowledgeDocumentTag>
{
    public void Configure(EntityTypeBuilder<KnowledgeDocumentTag> builder)
    {
        builder.ToTable("knowledge_document_tags");

        builder.HasKey(dt => new { dt.KnowledgeDocumentId, dt.TagId });

        builder.Property(dt => dt.CreatedAt)
               .HasColumnType("timestamptz")
               .HasDefaultValueSql("now()");

        builder.HasOne(dt => dt.KnowledgeDocument)
               .WithMany(d => d.Tags)
               .HasForeignKey(dt => dt.KnowledgeDocumentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(dt => dt.Tag)
               .WithMany(t => t.Documents)
               .HasForeignKey(dt => dt.TagId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
