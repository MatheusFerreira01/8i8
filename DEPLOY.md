# Guia de Deploy — Produção

## Pré-requisitos do servidor

- Ubuntu 22.04 LTS (mínimo 2 vCPU, 4GB RAM, 40GB disco)
- Domínio apontando para o IP do servidor (registro A configurado)
- Acesso SSH como root ou usuário com sudo

---

## 1. Preparar o servidor

```bash
# Atualizar sistema
apt update && apt upgrade -y

# Instalar dependências básicas
apt install -y curl git ufw fail2ban
```

---

## 2. Instalar Dockere
curl -fsSL https://get.docker.com | sh

# Adicionar seu usuário ao grupo docker (se não for root)
usermod -aG docker $USER
newgrp docker

# Verificar
docker --version
docker compose version
```

---

## 3. Configurar firewall (UFW)

```bash
ufw default deny incoming
ufw default allow outgoing
ufw allow ssh
ufw allow 80/tcp
ufw allow 443/tcp
ufw enable

# Verificar
ufw status
```

> **Importante:** portas do PostgreSQL, Redis e Evolution API (8080) NÃO devem ser abertas. Toda comunicação é via rede interna Docker.

---

## 4. Instalar Certbot (SSL)

```bash
apt install -y certbot

# Gerar certificado (substitua pelo seu domínio)
certbot certonly --standalone -d SEU_DOMINIO.COM

# Verificar
ls /etc/letsencrypt/live/SEU_DOMINIO.COM/
```

> O Nginx no Docker vai ler os certs via volume `/etc/letsencrypt`.

### Renovação automática

```bash
# Testar renovação
certbot renew --dry-run

# Criar cron para renovação automática
crontab -e
# Adicionar linha:
0 3 * * * certbot renew --quiet && docker restart 8i8-nginx
```

---

## 5. Clonar o repositório

```bash
cd /opt
git clone https://github.com/SEU_USUARIO/SEU_REPO.git 8i8
cd 8i8
```

---

## 6. Configurar variáveis de ambiente

```bash
cp .env.example .env
nano .env
```

Preencher obrigatoriamente:

| Variável | Valor |
|---|---|
| `POSTGRES_PASSWORD` | Senha forte (ex: `openssl rand -hex 32`) | 33286428212f86afa8e64e859055a6d09c2302abb10ce64bdfc4c1fe37cd5b18
| `REDIS_PASSWORD` | Senha forte | 05809ba84e004b33f1bb75726df6008108039306b1a38bb7782ac2ca86081bfa
| `AUTHENTICATION_API_KEY` | Chave forte para a Evolution API | dbe9b45f5aff0e289a2c79d1c31adb2bec26c8fe3a342bf13de9e4190fd37362
| `API_WEBHOOK_URL` | `https://SEU_DOMINIO.COM/webhook/messages` |
| `POSTGRES_DB` | Nome do banco (ex: `8i8_prod`) |
| `OLLAMA_MODEL_CHAT` | `llama3.2:1b` |
| `OLLAMA_MODEL_EMBED` | `nomic-embed-text` |

```bash
# Gerar senhas seguras
openssl rand -hex 32
```

---

## 7. Configurar o Nginx

```bash
# Substituir o placeholder pelo domínio real
sed -i 's/SEU_DOMINIO.COM/meudominio.com/g' nginx/nginx.conf
```

---

## 8. Build e subir os serviços

```bash
# Build da imagem da API
docker compose -f docker-compose.prod.yaml build

# Subir tudo
docker compose -f docker-compose.prod.yaml up -d

# Verificar containers
docker compose -f docker-compose.prod.yaml ps
```

---

## 9. Baixar o modelo Ollama

```bash
# Aguardar o Ollama subir (~10s) e fazer o pull
docker exec 8i8-ollama ollama pull llama3.2:1b
docker exec 8i8-ollama ollama pull nomic-embed-text

# Verificar modelos
docker exec 8i8-ollama ollama list
```

---

## 10. Verificar a API

```bash
# Health check
curl https://SEU_DOMINIO.COM/
curl https://8i8.network/
# Esperado:
# {"application":"8i8 API","environment":"Production","status":"Running"}
```

---

## 11. Testar criação de instância

```bash
curl -X POST https://SEU_DOMINIO.COM/instances/connect-phone \
  -H "Content-Type: application/json" \
  -d '{"name":"instancia-prod","phoneNumber":"5511999999999"}'
```

---

## Comandos úteis do dia a dia

```bash
# Ver logs da API
docker logs 8i8-api -f

# Ver logs da Evolution API
docker logs 8i8-evolution-prod -f

# Reiniciar apenas a API (após novo deploy)
docker compose -f docker-compose.prod.yaml build api
docker compose -f docker-compose.prod.yaml up -d api

# Parar tudo
docker compose -f docker-compose.prod.yaml down

# Backup do banco
docker exec 8i8-postgres-prod pg_dump -U postgres 8i8_prod > backup_$(date +%Y%m%d).sql
```

---

## Novo deploy (atualizar versão)

```bash
cd /opt/8i8
git pull origin main

docker compose -f docker-compose.prod.yaml build api
docker compose -f docker-compose.prod.yaml up -d api

# Verificar se subiu sem erro
docker logs 8i8-api --tail 50
```

---

## Troubleshooting

**API não sobe**
```bash
docker logs 8i8-api
# Verificar connection string do Postgres e variáveis de ambiente
```

**Webhook não chega**
```bash
# Verificar se a porta 443 está aberta
curl -I https://SEU_DOMINIO.COM/webhook/messages

# Verificar logs do Nginx
docker logs 8i8-nginx
```

**Evolution API não conecta ao banco**
```bash
docker logs 8i8-evolution-prod
# Confirmar DATABASE_CONNECTION_URI no .env
```

**Certificado SSL expirado**
```bash
certbot renew --force-renewal
docker restart 8i8-nginx
```
