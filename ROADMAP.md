# Roadmap — Próximos Passos

## Prioridade 1 — Segurança (antes do go-live com clientes reais)

### 1.1 Autenticação nas rotas de instância
**Problema:** `POST /instances`, `GET /instances/*` estão abertas. Qualquer um com a URL pode criar instâncias.

**Solução:** Middleware de API Key no grupo `/instances`.
- Adicionar `X-Api-Key` header obrigatório
- Chave configurada via `appsettings` / env var
- Retorna `401` se ausente ou inválida

**Arquivos:** `Program.cs`, novo `ApiKeyMiddleware.cs`

---

### 1.2 Validação de assinatura do webhook
**Problema:** Qualquer request POST em `/webhook/messages` é processada. Um atacante pode injetar eventos falsos.

**Solução:** Evolution API envia um token no header `apikey` de cada webhook. Validar esse header antes de processar.
- Comparar `Request.Headers["apikey"]` com `AUTHENTICATION_API_KEY` do `.env`
- Retorna `401` se inválido

**Arquivo:** `WebhookEndpoints.cs`

---

### 1.3 CORS restrito
**Problema:** `AllowAnyOrigin()` permite requests de qualquer domínio.

**Solução:** Restringir para o domínio do frontend em produção.
```csharp
policy.WithOrigins("https://SEU_DOMINIO.COM")
```

**Arquivo:** `Program.cs`

---

## Prioridade 2 — Fluxo de conversa

### 2.1 Persistir conversas e mensagens no banco
**Problema:** Mensagens chegam, são processadas e desaparecem. Sem histórico, sem auditoria, sem deduplicação.

**Solução:** Ao receber mensagem, salvar/buscar `Conversation` (por número + instância) e registrar `Message` (inbound). Ao responder, registrar `Message` (outbound).
- Usar `ExternalMessageId` para evitar reprocessar a mesma mensagem duas vezes

**Arquivos:** `WebhookEndpoints.cs`, `AppDbContext`

---

### 2.2 Capturar seleção do usuário (número do menu)
**Problema:** Hoje qualquer mensagem retorna o menu. Não há estado de conversa — usuário digita "1" e recebe o menu de novo.

**Solução:** Verificar se a última mensagem da conversa foi o menu de categorias. Se sim, tentar parsear a resposta como número e direcionar para o fluxo da categoria escolhida.

**Fluxo:**
```
Mensagem recebida
    ↓
Busca última mensagem da conversa
    ↓
Se última foi menu → parsear número → direcionar para categoria
Se não → enviar menu
```

**Arquivos:** `WebhookEndpoints.cs`, novo `ConversationStateService.cs`

---

### 2.3 Integrar Ollama no fluxo por categoria
**Problema:** Ollama está configurado mas não é usado na versão MVP.

**Solução:** Após usuário escolher categoria, Ollama responde com base nos documentos daquela categoria (RAG com pgvector).

**Fluxo:**
```
Usuário escolhe "Financeiro"
    ↓
Busca documentos da categoria "financeiro" via pgvector (similaridade semântica)
    ↓
Ollama responde baseado nos documentos + pergunta do usuário
```

**Arquivos:** `OllamaService.cs`, nova rota de upload de documentos

---

## Prioridade 3 — Robustez

### 3.1 Retry com backoff no Evolution API client
**Problema:** Se `SendTextMessageAsync` falhar (Evolution API reiniciando), a resposta some sem aviso.

**Solução:** Adicionar Polly com retry exponencial (3 tentativas, 1s/2s/4s).
```bash
dotnet add package Polly
dotnet add package Microsoft.Extensions.Http.Polly
```

**Arquivo:** `Program.cs` (configurar na pipeline do `AddHttpClient`)

---

### 3.2 Health checks
**Solução:** Endpoint `/health` verificando Postgres, Evolution API e Ollama.
```csharp
builder.Services.AddHealthChecks()
    .AddNpgsql(connectionString)
    .AddUrlGroup(new Uri(ollamaUrl), "ollama")
    .AddUrlGroup(new Uri(evolutionUrl), "evolution");
```

---

### 3.3 Structured logging com correlação
**Problema:** Logs atuais não correlacionam eventos de uma mesma conversa.

**Solução:** Adicionar `PhoneNumber` e `Instance` como propriedades enriquecidas no Serilog para cada request de webhook.

---

## Prioridade 4 — Features

### 4.1 Upload e indexação de documentos por categoria
Endpoint para fazer upload de documentos (PDF/texto) associados a uma categoria. Pipeline:
1. Receber documento
2. Chunking do texto
3. Gerar embeddings via Ollama (`nomic-embed-text`)
4. Salvar em `DocumentEmbedding` com pgvector

### 4.2 Painel de administração
Interface web para:
- Gerenciar instâncias WhatsApp
- Gerenciar categorias e documentos
- Ver histórico de conversas
- Monitorar status de conexão

### 4.3 Multi-tenant
Suporte a múltiplos clientes/empresas, cada um com suas instâncias, categorias e documentos isolados.

### 4.4 Transferência para humano
Após N interações sem resolução, ou quando usuário solicitar, abrir um ticket ou notificar um atendente humano (integração Chatwoot / n8n).

---

## Ordem sugerida de execução

```
[x] MVP — menu de categorias funcional
[ ] 1.2 Validação de assinatura webhook
[ ] 2.1 Persistir conversas
[ ] 2.2 Capturar seleção do menu
[ ] 1.1 Autenticação nas rotas de instância
[ ] 3.1 Retry com Polly
[ ] 2.3 Ollama + RAG por categoria
[ ] 3.2 Health checks
[ ] 4.1 Upload de documentos
[ ] 4.2 Painel admin
```
