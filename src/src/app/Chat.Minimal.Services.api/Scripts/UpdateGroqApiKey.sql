-- Atualizar API Key do Groq
UPDATE AiConfigs 
SET ApiKey = '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',
    UpdatedAt = GETUTCDATE()
WHERE Provider = 'Groq';

-- Verificar atualização
SELECT 
    Id,
    Name,
    Provider,
    Model,
    LEFT(ApiKey, 20) + '...' AS ApiKeyPreview,
    TokenLimit,
    TokensUsed,
    ExpiresAt,
    IsActive,
    UpdatedAt
FROM AiConfigs
WHERE Provider = 'Groq';
