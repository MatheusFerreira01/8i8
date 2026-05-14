using ConnectAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConnectAi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    public DbSet<KnowledgeDocument> KnowledgeDocuments => Set<KnowledgeDocument>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<KnowledgeDocumentTag> KnowledgeDocumentTags => Set<KnowledgeDocumentTag>();
    public DbSet<DocumentEmbedding> DocumentEmbeddings => Set<DocumentEmbedding>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("pgcrypto");
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
