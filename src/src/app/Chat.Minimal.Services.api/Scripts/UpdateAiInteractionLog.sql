-- Adicionar coluna StatusCode na tabela AiInteractionLogs
IF NOT EXISTS (
    SELECT * 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[AiInteractionLogs]') 
    AND name = 'StatusCode'
)
BEGIN
    ALTER TABLE [AiInteractionLogs] ADD [StatusCode] int NULL;
END
GO
