using ConnectAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConnectAi.Infrastructure.Persistence.Configurations;

public class DocumentEmbeddingConfiguration : IEntityTypeConfiguration<DocumentEmbedding>
{
    public void Configure(EntityTypeBuilder<DocumentEmbedding> builder)
    {
        builder.ToTable("document_embeddings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.ChunkIndex)
               .IsRequired();

        builder.Property(e => e.ChunkText)
               .IsRequired();

        // vector(768) = nomic-embed-text (Ollama). Ajuste se mudar modelo.
        builder.Property(e => e.Embedding)
               .HasColumnType("vector(768)");

        builder.Property(e => e.Model)
               .HasMaxLength(128);

        builder.Property(e => e.CreatedAt)
               .HasColumnType("timestamptz")
               .HasDefaultValueSql("now()");

        builder.HasIndex(e => e.KnowledgeDocumentId);

        // Índice HNSW para busca semântica ANN via pgvector
        builder.HasIndex(e => e.Embedding)
               .HasMethod("hnsw")
               .HasOperators("vector_cosine_ops");
    }
}
