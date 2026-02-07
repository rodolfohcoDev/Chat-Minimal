-- Script para inserir configuração inicial do Groq no banco de dados SQL Server

-- Inserir configuração do Groq
INSERT INTO AiConfigs (Name, Provider, Model, ApiKey, BaseUrl, TokenLimit, TokensUsed, ExpiresAt, IsActive, CreatedAt)
VALUES (
    'Groq Default',
    'Groq',
    'llama-3.3-70b-versatile',
    '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',
    'https://api.groq.com/openai/v1',
    1000000, -- 1 milhão de tokens de limite
    0,       -- Nenhum token usado ainda
    DATEADD(YEAR, 1, GETUTCDATE()), -- Expira em 1 ano
    1,       -- Ativo
    GETUTCDATE()
);

-- Verificar se foi inserido
SELECT * FROM AiConfigs;
