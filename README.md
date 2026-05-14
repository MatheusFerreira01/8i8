# 8i8 — WhatsApp Atendimento Virtual

API de atendimento via WhatsApp com integração Evolution API e IA local (Ollama).

## Stack

- **.NET 9** — ASP.NET Core Minimal API
- **PostgreSQL 16 + pgvector** — banco de dados + embeddings semânticos
- **Evolution API v2** — gateway WhatsApp (Baileys)
- **Ollama** — IA local para respostas (llama3.2:1b)
- **Redis** — cache (disponível para uso futuro)
- **Docker Compose** — ambiente de desenvolvimento

## Funcionalidades do MVP

- Criação de instâncias WhatsApp via QR code ou código de pareamento (número de telefone)
- Configuração automática de webhook ao criar instância
- Recebimento de mensagens via webhook
- Resposta automática com menu de categorias de atendimento
- Validação de estado de conexão antes de processar mensagens
- Seed automático de categorias no banco

## Pré-requisitos

- Docker + Docker Compose
- .NET 9 SDK
- Ollama rodando localmente (`http://localhost:11434`)

## Setup

### 1. Variáveis de ambiente

```bash
cp .env.example .env
```

Edite `.env` com suas configurações.

### 2. Subir infraestrutura

```bash
docker compose -f docker-compose.dev.yaml up -d
```

### 3. Modelo de IA

```bash
# Via Ollama CLI
ollama pull llama3.2:1b

# Ou via API
curl -X POST http://localhost:11434/api/pull -d '{"name":"llama3.2:1b"}'
```

### 4. Rodar a API

```bash
dotnet run --project 8i8.Api
```

A API sobe em `http://localhost:5000`. Swagger disponível em `http://localhost:5000/swagger`.

Migrations e seed rodam automaticamente no startup.

## Rotas

### Instâncias

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/instances` | Cria instância + retorna QR code |
| `POST` | `/instances/connect-phone` | Cria instância + conecta via código de pareamento |
| `GET` | `/instances/{name}/qrcode` | Retorna QR code atualizado |
| `GET` | `/instances/{name}/status` | Verifica estado da conexão |

#### POST /instances
```json
{ "name": "minha-instancia" }
```

#### POST /instances/connect-phone
```json
{
  "name": "minha-instancia",
  "phoneNumber": "5511999999999"
}
```
> Número no formato internacional sem `+`: `55` + DDD + número

### Webhook

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/webhook/messages` | Recebe eventos da Evolution API |

Configurado automaticamente ao criar instâncias. Processa `MESSAGES_UPSERT`, `CONNECTION_UPDATE` e `QRCODE_UPDATED`.

## Fluxo de atendimento

```
Cliente envia mensagem
        ↓
Webhook recebe evento
        ↓
Valida estado da instância (open)
        ↓
Busca categorias no banco
        ↓
Envia menu de opções via WhatsApp
```

## Configuração (appsettings)

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=...;Database=...;Username=...;Password=..."
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "ChatModel": "llama3.2:1b",
    "EmbedModel": "nomic-embed-text",
    "NumPredict": 300,
    "NumCtx": 512
  },
  "EvolutionApi": {
    "BaseUrl": "http://localhost:8080",
    "ApiKey": "sua-api-key",
    "WebhookUrl": "http://host.docker.internal:5000/webhook/messages"
  }
}
```

> Em produção, `WebhookUrl` deve apontar para a URL pública da API.

## Estrutura do projeto

```
8i8/
├── 8i8.Api/            # Endpoints, models, configuração
├── 8i8.Domain/         # Entidades e enums
├── 8i8.Infrastructure/ # EF Core, Ollama, Evolution API client
├── 8i8.Application/    # (reservado para use cases)
├── 8i8.Contracts/      # (reservado para DTOs públicos)
└── docker-compose.dev.yaml
```
