namespace ConnectAi.Domain.Entities;

/// <summary>
/// Categoria/tag aplicável a um KnowledgeDocument.
/// Exemplos: financeiro, suporte, RH, comercial, boletos, horário de atendimento.
/// </summary>
public class Tag
{
    public Guid Id { get; set; }

    /// <summary>Nome amigável da tag (ex.: "Financeiro").</summary>
    public string Name { get; set; } = default!;

    /// <summary>Versão normalizada/identificadora (ex.: "financeiro"), única.</summary>
    public string Slug { get; set; } = default!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<KnowledgeDocumentTag> Documents { get; set; } = new List<KnowledgeDocumentTag>();
}
