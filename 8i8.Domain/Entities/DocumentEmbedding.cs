using Pgvector;

namespace ConnectAi.Domain.Entities;

/// <summary>
/// Chunk vetorizado de um KnowledgeDocument. Cada documento pode ter N chunks,
/// cada chunk gera um embedding que é indexado por pgvector para busca semântica.
/// </summary>
public class DocumentEmbedding
{
    public Guid Id { get; set; }

    public Guid KnowledgeDocumentId { get; set; }
    public KnowledgeDocument KnowledgeDocument { get; set; } = default!;

    /// <summary>Ordem do chunk dentro do documento (0..N-1).</summary>
    public int ChunkIndex { get; set; }

    /// <summary>Texto do chunk (recorte do Content do documento).</summary>
    public string ChunkText { get; set; } = default!;

    /// <summary>
    /// Vetor de embeddings (pgvector). Dimensão default: 768
    /// (compatível com nomic-embed-text do Ollama). Ajuste o tamanho na
    /// configuração se for usar outro modelo (1536 para OpenAI ada-002, etc).
    /// </summary>
    public Vector? Embedding { get; set; }

    /// <summary>Modelo que gerou o embedding (ex.: "nomic-embed-text").</summary>
    public string? Model { get; set; }

    public DateTime CreatedAt { get; set; }
}
