-- Script para criar usuário e API Key de teste
USE chatminimaldb;

-- Criar um usuário de teste
SET @userId = UUID();
SET @normalizedEmail = UPPER('testuser@example.com');
SET @normalizedUserName = UPPER('testuser');

INSERT INTO AspNetUsers (
    Id, 
    UserName, 
    NormalizedUserName, 
    Email, 
    NormalizedEmail, 
    EmailConfirmed, 
    PasswordHash,
    SecurityStamp,
    ConcurrencyStamp,
    PhoneNumberConfirmed,
    TwoFactorEnabled,
    LockoutEnabled,
    AccessFailedCount,
    CreatedAt,
    IsActive
)
VALUES (
    @userId,
    'testuser',
    @normalizedUserName,
    'testuser@example.com',
    @normalizedEmail,
    1,
    'AQAAAAIAAYagAAAAEDummyHashForTestingPurposesOnly==', -- Hash dummy (não use em produção)
    CONCAT(UUID(), UUID()),
    UUID(),
    0,
    0,
    0,
    0,
    NOW(),
    1
);

-- Inserir API Key de teste (válida por 2 anos)
INSERT INTO ApiKeys (Id, `Key`, Name, CreatedAt, ExpiresAt, IsActive, UserId)
VALUES (
    UUID(),
    'test-api-key-12345678901234567890123456789012',
    'Test API Key',
    NOW(),
    DATE_ADD(NOW(), INTERVAL 2 YEAR),
    1,
    @userId
);

-- Exibir a API Key criada
SELECT `Key`, Name, CreatedAt, ExpiresAt, IsActive 
FROM ApiKeys 
WHERE Name = 'Test API Key';
