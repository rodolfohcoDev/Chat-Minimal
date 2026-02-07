-- Atualizar modelo do Groq para llama-3.1-8b-instant
UPDATE AiConfigs 
SET Model = 'llama-3.1-8b-instant',
    UpdatedAt = GETUTCDATE()
WHERE Provider = 'Groq';
