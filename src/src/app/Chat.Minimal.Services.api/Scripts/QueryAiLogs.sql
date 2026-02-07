-- Script para consultar logs de interação com IA

-- 1. Ver todas as configurações de IA
SELECT 
    Id,
    Name,
    Provider,
    Model,
    TokenLimit,
    TokensUsed,
    CAST(TokensUsed AS FLOAT) / CAST(TokenLimit AS FLOAT) * 100 AS PercentUsed,
    ExpiresAt,
    IsActive,
    CreatedAt
FROM AiConfigs
ORDER BY CreatedAt DESC;

-- 2. Ver todos os logs de interação
SELECT 
    Id,
    AiConfigId,
    ConversationId,
    UserId,
    LEFT(RequestContent, 100) AS RequestPreview,
    LEFT(ResponseContent, 100) AS ResponsePreview,
    InputTokens,
    OutputTokens,
    InputTokens + OutputTokens AS TotalTokens,
    DurationMs,
    Status,
    ErrorMessage,
    Timestamp
FROM AiInteractionLogs
ORDER BY Timestamp DESC;

-- 3. Estatísticas de uso por configuração
SELECT 
    ac.Name AS ConfigName,
    ac.Provider,
    ac.Model,
    COUNT(ail.Id) AS TotalInteractions,
    SUM(ail.InputTokens) AS TotalInputTokens,
    SUM(ail.OutputTokens) AS TotalOutputTokens,
    SUM(ail.InputTokens + ail.OutputTokens) AS TotalTokens,
    AVG(ail.DurationMs) AS AvgDurationMs,
    SUM(CASE WHEN ail.Status = 'Success' THEN 1 ELSE 0 END) AS SuccessCount,
    SUM(CASE WHEN ail.Status = 'Error' THEN 1 ELSE 0 END) AS ErrorCount
FROM AiConfigs ac
LEFT JOIN AiInteractionLogs ail ON ac.Id = ail.AiConfigId
GROUP BY ac.Id, ac.Name, ac.Provider, ac.Model
ORDER BY TotalInteractions DESC;

-- 4. Últimas 10 interações com detalhes completos
SELECT TOP 10
    ail.Id,
    ail.ConversationId,
    ac.Name AS ConfigName,
    ac.Provider,
    ac.Model,
    ail.RequestContent,
    ail.ResponseContent,
    ail.InputTokens,
    ail.OutputTokens,
    ail.InputTokens + ail.OutputTokens AS TotalTokens,
    ail.DurationMs,
    ail.Status,
    ail.ErrorMessage,
    ail.Timestamp
FROM AiInteractionLogs ail
INNER JOIN AiConfigs ac ON ail.AiConfigId = ac.Id
ORDER BY ail.Timestamp DESC;
