# 🤖 Configuração do Modelo GGUF para Docker

## 📥 Download do Modelo

A aplicação está configurada para usar o modelo **Llama 3.2 3B Instruct** em formato GGUF.

### Opção 1: Download via Hugging Face (Recomendado)

```bash
# Criar pasta models se não existir
mkdir models

# Baixar modelo (escolha um dos links abaixo)
```

**Links para Download:**

1. **Llama 3.2 3B Instruct Q4_K_M** (~2.0 GB)
   - URL: https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf
   - Comando:
   ```bash
   curl -L "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf" -o models/llama-3.2-3b-instruct.Q4_K_M.gguf
   ```

2. **Llama 3.2 3B Instruct Q5_K_M** (~2.3 GB - Melhor qualidade)
   - URL: https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q5_K_M.gguf

3. **Llama 3.2 3B Instruct Q3_K_M** (~1.5 GB - Mais rápido)
   - URL: https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q3_K_M.gguf

### Opção 2: Usar outro modelo GGUF

Se você já tem um modelo GGUF, coloque-o na pasta `models/` e atualize o `docker-compose.yml`:

```yaml
environment:
  - GgufModelSettings__ModelPath=/app/models/SEU-MODELO.gguf
```

## 🔧 Estrutura de Pastas

```
Chat_micro/
├── models/                          # ← Coloque os modelos aqui
│   └── llama-3.2-3b-instruct.Q4_K_M.gguf
├── docker-compose.yml
└── ...
```

## 🚀 Após Baixar o Modelo

1. **Rebuild da imagem Docker** (se necessário):
   ```bash
   docker build -t chat-minimal-api:latest .
   ```

2. **Reiniciar containers**:
   ```bash
   docker-compose down
   docker-compose up -d
   ```

3. **Verificar logs**:
   ```bash
   docker logs chat-minimal-api -f
   ```

   Você deve ver:
   ```
   Carregando modelo GGUF...
   Modelo carregado com sucesso!
   ```

## 🧪 Testar a IA

```bash
curl -X POST http://localhost:5120/api/chat/task \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: test-api-key-12345678901234567890123456789012" \
  -d @scripts/test-docker-chat.json
```

## ⚙️ Configurações Avançadas

### Ajustar Threads e GPU

Edite `appsettings.Production.json`:

```json
"GgufModelSettings": {
  "ModelPath": "/app/models/llama-3.2-3b-instruct.Q4_K_M.gguf",
  "ContextSize": 4096,
  "GpuLayerCount": 0,      // ← Aumente para usar GPU (se disponível)
  "Threads": 4,            // ← Ajuste conforme CPU
  "Seed": 1337,
  "Verbose": true          // ← true para debug
}
```

### Usar GPU no Docker

Para usar GPU NVIDIA no Docker:

1. Instale NVIDIA Container Toolkit
2. Atualize `docker-compose.yml`:
   ```yaml
   app:
     deploy:
       resources:
         reservations:
           devices:
             - driver: nvidia
               count: 1
               capabilities: [gpu]
   ```
3. Aumente `GpuLayerCount` no appsettings

## 📊 Tamanhos e Performance

| Quantização | Tamanho | RAM Mínima | Velocidade | Qualidade |
|-------------|---------|------------|------------|-----------|
| Q3_K_M      | ~1.5 GB | 3 GB       | ⚡⚡⚡      | ⭐⭐      |
| Q4_K_M      | ~2.0 GB | 4 GB       | ⚡⚡        | ⭐⭐⭐    |
| Q5_K_M      | ~2.3 GB | 5 GB       | ⚡          | ⭐⭐⭐⭐  |
| Q8_0        | ~3.5 GB | 6 GB       | ⚡          | ⭐⭐⭐⭐⭐|

**Recomendação**: Q4_K_M oferece o melhor equilíbrio entre qualidade e performance.

## ❌ Troubleshooting

### Erro: "Modelo GGUF não encontrado"
- Verifique se o arquivo está em `models/`
- Confirme o nome do arquivo no `docker-compose.yml`
- Reinicie os containers

### Erro: "Out of memory"
- Use modelo menor (Q3_K_M)
- Reduza `ContextSize`
- Aumente RAM do Docker Desktop

### Modelo carrega mas não responde
- Verifique logs: `docker logs chat-minimal-api`
- Aumente `Threads` se CPU estiver ociosa
- Teste com `Verbose: true`

## 🔗 Links Úteis

- Hugging Face GGUF Models: https://huggingface.co/models?search=gguf
- LlamaCpp Documentation: https://github.com/ggerganov/llama.cpp
- Quantization Guide: https://github.com/ggerganov/llama.cpp/blob/master/examples/quantize/README.md
