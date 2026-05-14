namespace ConnectAi.Domain.Entities;

/// <summary>
/// Documento da base de conhecimento (KB). É o núcleo do RAG:
/// cada documento pode ser categorizado por múltiplas tags e
/// quebrado em vários chunks vetorizados (DocumentEmbedding).
/// </summary>
public class KnowledgeDocument
{
    public Guid Id { get; set; }

    /// <summary>Título curto/identificador da informação.</summary>
    public string Title { get; set; } = default!;

    /// <summary>Conteúdo textual completo (markdown ou texto puro).</summary>
    public string Content { get; set; } = default!;

    /// <summary>
    /// Origem da informação (ex.: "manual", "url:https://...", "pdf:contrato.pdf").
    /// Útil para auditoria e citações.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>Se está disponível para ser consultado pela IA.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<KnowledgeDocumentTag> Tags { get; set; } = new List<KnowledgeDocumentTag>();
    public ICollection<DocumentEmbedding> Embeddings { get; set; } = new List<DocumentEmbedding>();
}
