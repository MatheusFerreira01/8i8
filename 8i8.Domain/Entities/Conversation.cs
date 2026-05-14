using ConnectAi.Domain.Enums;

namespace ConnectAi.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; }

    /// <summary>
    /// Número do WhatsApp (E.164 sem o @s.whatsapp.net), ex: "5511999999999".
    /// </summary>
    public string PhoneNumber { get; set; } = default!;

    /// <summary>
    /// Nome do contato no WhatsApp (pushName), quando disponível.
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// Nome da instância Evolution que originou a conversa (ex.: "DevTests").
    /// </summary>
    public string? InstanceName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
