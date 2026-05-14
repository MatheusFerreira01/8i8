namespace ConnectAi.Domain.Enums;

public enum MessageDirection
{
    /// <summary>Mensagem recebida do usuário via WhatsApp.</summary>
    Inbound = 1,

    /// <summary>Mensagem enviada pelo bot ao usuário.</summary>
    Outbound = 2
}
