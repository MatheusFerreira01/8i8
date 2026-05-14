namespace ConnectAi.Infrastructure.AI;

public class OllamaSettings
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ChatModel { get; set; } = "llama3.2";
    public string EmbedModel { get; set; } = "nomic-embed-text";
    public int NumPredict { get; set; } = 300;
    public int NumCtx { get; set; } = 512;
}
