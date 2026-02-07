-- Script para criar usuário e chave de API para consumo do serviço de IA

-- Variáveis
DECLARE @UserId NVARCHAR(450);
SET @UserId = CAST(NEWID() AS NVARCHAR(450));
DECLARE @ApiKey NVARCHAR(MAX); 
SET @ApiKey = 'sk-prod-' + LOWER(REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', ''));
DECLARE @UserName NVARCHAR(256) = 'consumidor-ia@sistema.com';

-- 1. Inserir Usuário (AspNetUsers)
-- Nota: PasswordHash é fictício. O usuário deve ser gerenciado via Identity API se precisar de login.
-- Para consumo via API Key, o usuário serve como proprietário da chave.
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = @UserName)
BEGIN
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail, 
        EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, 
        PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount
    )
    VALUES (
        @UserId, 
        @UserName, 
        UPPER(@UserName), 
        @UserName, 
        UPPER(@UserName), 
        1, 
        'AQAAAAIAAYagAAAAEPcw...', -- Hash placeholder
        CAST(NEWID() AS NVARCHAR(36)), 
        CAST(NEWID() AS NVARCHAR(36)), 
        0, 0, 1, 0
    );
    
    PRINT 'Usuário criado com ID: ' + @UserId;
END
ELSE
BEGIN
    SELECT @UserId = Id FROM AspNetUsers WHERE UserName = @UserName;
    PRINT 'Usuário já existe. ID: ' + @UserId;
END

-- 2. Inserir API Key
IF NOT EXISTS (SELECT 1 FROM ApiKeys WHERE UserId = @UserId AND Name = 'Chave Produção IA')
BEGIN
    INSERT INTO ApiKeys (Id, [Key], Name, CreatedAt, ExpiresAt, IsActive, UserId)
    VALUES (
        NEWID(),
        @ApiKey,
        'Chave Produção IA',
        GETUTCDATE(),
        DATEADD(YEAR, 1, GETUTCDATE()), -- Expira em 1 ano
        1,
        @UserId
    );
    
    PRINT 'API Key criada com sucesso!';
    SELECT @ApiKey AS GeneratedApiKey, @UserId AS OwnerUserId, @UserName AS OwnerEmail;
END
ELSE
BEGIN
    PRINT 'Usuário já possui esta chave.';
    SELECT [Key] AS ExistingApiKey FROM ApiKeys WHERE UserId = @UserId AND Name = 'Chave Produção IA';
END
