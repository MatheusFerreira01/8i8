using ConnectAi.Domain.Enums;

namespace ConnectAi.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = default!;

    /// <summary>
    /// Direção da mensagem: Inbound (recebida do usuário) ou Outbound (enviada pelo bot).
    /// </summary>
    public MessageDirection Direction { get; set; }

    /// <summary>
    /// Conteúdo textual da mensagem.
    /// </summary>
    public string Content { get; set; } = default!;

    /// <summary>
    /// ID externo da mensagem na Evolution API / WhatsApp (key.id).
    /// Usado para deduplicação caso o webhook seja entregue mais de uma vez.
    /// </summary>
    public string? ExternalMessageId { get; set; }

    /// <summary>
    /// Quando a mensagem foi enviada (timestamp do WhatsApp).
    /// </summary>
    public DateTime SentAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
