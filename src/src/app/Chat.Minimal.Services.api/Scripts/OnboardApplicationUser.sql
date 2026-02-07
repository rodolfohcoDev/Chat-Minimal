-- Script de Onboarding de Aplicação
-- Cria roles, usuários e API Key com permissões específicas

-- 1. Criar Roles (Administrator e application_user)
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Administrator')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Administrator', 'ADMINISTRATOR', NEWID());
    PRINT 'Role Administrator criada.';
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'application_user')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'application_user', 'APPLICATION_USER', NEWID());
    PRINT 'Role application_user criada.';
END

-- 2. Criar Usuário Admin
DECLARE @AdminId NVARCHAR(450);
SET @AdminId = (SELECT Id FROM AspNetUsers WHERE UserName = 'admin@sistema.com');

IF @AdminId IS NULL
BEGIN
    SET @AdminId = CAST(NEWID() AS NVARCHAR(450));
    -- Senha padrão: Admin@123 (hash fictício, substitua por hash real gerado pelo UserManager)
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, CreatedAt, IsActive)
    VALUES (@AdminId, 'admin@sistema.com', 'ADMIN@SISTEMA.COM', 'admin@sistema.com', 'ADMIN@SISTEMA.COM', 1, 
    'AQAAAAIAAYagAAAAEPcw...', -- Hash placeholder
    NEWID(), NEWID(), 0, 0, 1, 0, GETUTCDATE(), 1);
    PRINT 'Usuário admin criado.';
END

-- Associar Admin Role
DECLARE @AdminRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Administrator');
IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @AdminId AND RoleId = @AdminRoleId)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AdminId, @AdminRoleId);
    PRINT 'Admin associado à role Administrator.';
END

-- 3. Criar Usuário de Aplicação (Service Account)
DECLARE @AppUserId NVARCHAR(450);
SET @AppUserId = (SELECT Id FROM AspNetUsers WHERE UserName = 'app-integration@sistema.com');

IF @AppUserId IS NULL
BEGIN
    SET @AppUserId = CAST(NEWID() AS NVARCHAR(450));
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, CreatedAt, IsActive)
    VALUES (@AppUserId, 'app-integration@sistema.com', 'APP-INTEGRATION@SISTEMA.COM', 'app-integration@sistema.com', 'APP-INTEGRATION@SISTEMA.COM', 1, 
    'AQAAAAIAAYagAAAAEPcw...', -- Hash placeholder
    NEWID(), NEWID(), 0, 0, 1, 0, GETUTCDATE(), 1);
    PRINT 'Usuário de aplicação criado.';
END

-- Associar Application User Role
DECLARE @AppRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'application_user');
IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @AppUserId AND RoleId = @AppRoleId)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AppUserId, @AppRoleId);
    PRINT 'App User associado à role application_user.';
END

-- 4. Criar API Key para o Usuário de Aplicação (Permissão de Acesso)
DECLARE @ApiKey NVARCHAR(MAX);
SET @ApiKey = 'sk-app-' + LOWER(REPLACE(CAST(NEWID() AS NVARCHAR(36)), '-', ''));

IF NOT EXISTS (SELECT 1 FROM ApiKeys WHERE UserId = @AppUserId AND Name = 'App Integration Key')
BEGIN
    INSERT INTO ApiKeys (Id, [Key], Name, CreatedAt, ExpiresAt, IsActive, UserId)
    VALUES (NEWID(), @ApiKey, 'App Integration Key', GETUTCDATE(), DATEADD(YEAR, 1, GETUTCDATE()), 1, @AppUserId);
    
    PRINT '---------------------------------------------------';
    PRINT 'ONBOARDING CONCLUÍDO COM SUCESSO';
    PRINT '---------------------------------------------------';
    PRINT 'Admin User: admin@sistema.com';
    PRINT 'App User:   app-integration@sistema.com';
    PRINT 'App Role:   application_user';
    PRINT 'API Key:    ' + @ApiKey;
    PRINT '---------------------------------------------------';
    
    -- Retornar a chave gerada para o DBA/DevOps ver
    SELECT @ApiKey AS GeneratedApiKey, @AppUserId AS UserId;
END
ELSE
BEGIN
    PRINT 'API Key já existe.';
    SELECT [Key] AS ExistingApiKey FROM ApiKeys WHERE UserId = @AppUserId AND Name = 'App Integration Key';
END
