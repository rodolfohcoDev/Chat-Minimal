-- Variable declarations need to be handled carefully in batch scripts passed to docker exec
-- We will simply perform the inserts.

-- 1. Create the user (using INSERT IGNORE to skip if exists, but we need the ID)
-- Since variables across statements can be tricky in some SQL execution contexts via docker exec,
-- we will use a deterministic UUID for the user or lookup.
-- Let's try to set a variable.

SET @newUserId = UUID();
SET @email = 'rodolfohco@hotmail.com';
SET @userName = 'rodolfohco';

INSERT IGNORE INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, 
    TwoFactorEnabled, LockoutEnabled, AccessFailedCount, CreatedAt, IsActive
)
VALUES (
    @newUserId,
    @userName,
    UPPER(@userName),
    @email,
    UPPER(@email),
    1,
    'AQAAAAIAAYagAAAAEDummyHashForTestingPurposesOnly==',
    UUID(),
    UUID(),
    0, 0, 0, 0,
    NOW(),
    1
);

-- Ensure we have the correct UserId (if it was inserted or already existed)
SET @targetUserId = (SELECT Id FROM AspNetUsers WHERE NormalizedEmail = UPPER(@email) LIMIT 1);

-- 2. Create the API Key
INSERT INTO ApiKeys (Id, `Key`, Name, CreatedAt, ExpiresAt, IsActive, UserId)
VALUES (
    UUID(), 
    'apikey-12345678901234567890123456789012', 
    'Aplicacao Demonstracao', 
    NOW(), 
    DATE_ADD(NOW(), INTERVAL 2 YEAR), 
    1, 
    @targetUserId
);

SELECT * FROM ApiKeys WHERE Name = 'Aplicacao Demonstracao';
