namespace ConnectAi.Domain.Entities;

/// <summary>
/// Tabela de junção N:N entre KnowledgeDocument e Tag.
/// </summary>
public class KnowledgeDocumentTag
{
    public Guid KnowledgeDocumentId { get; set; }
    public KnowledgeDocument KnowledgeDocument { get; set; } = default!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = default!;

    public DateTime CreatedAt { get; set; }
}
