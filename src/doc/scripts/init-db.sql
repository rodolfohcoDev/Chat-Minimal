-- Script para inicializar o banco de dados com as tabelas e dados de teste

-- Criar tabela de Roles
CREATE TABLE IF NOT EXISTS `AspNetRoles` (
    `Id` varchar(255) NOT NULL,
    `Name` varchar(256) DEFAULT NULL,
    `NormalizedName` varchar(256) DEFAULT NULL,
    `ConcurrencyStamp` longtext,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `RoleNameIndex` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Criar tabela de Users
CREATE TABLE IF NOT EXISTS `AspNetUsers` (
    `Id` varchar(255) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) DEFAULT NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT '1',
    `UserName` varchar(256) DEFAULT NULL,
    `NormalizedUserName` varchar(256) DEFAULT NULL,
    `Email` varchar(256) DEFAULT NULL,
    `NormalizedEmail` varchar(256) DEFAULT NULL,
    `EmailConfirmed` tinyint(1) NOT NULL,
    `PasswordHash` longtext,
    `SecurityStamp` longtext,
    `ConcurrencyStamp` longtext,
    `PhoneNumber` longtext,
    `PhoneNumberConfirmed` tinyint(1) NOT NULL,
    `TwoFactorEnabled` tinyint(1) NOT NULL,
    `LockoutEnd` datetime(6) DEFAULT NULL,
    `LockoutEnabled` tinyint(1) NOT NULL,
    `AccessFailedCount` int NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UserNameIndex` (`NormalizedUserName`),
    KEY `EmailIndex` (`NormalizedEmail`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Criar tabela de API Keys
CREATE TABLE IF NOT EXISTS `ApiKeys` (
    `Id` char(36) NOT NULL,
    `Key` varchar(256) NOT NULL,
    `Name` varchar(100) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `ExpiresAt` datetime(6) DEFAULT NULL,
    `IsActive` tinyint(1) NOT NULL DEFAULT '1',
    `UserId` varchar(255) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `IX_ApiKeys_Key` (`Key`),
    KEY `IX_ApiKeys_UserId` (`UserId`),
    CONSTRAINT `FK_ApiKeys_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Criar outras tabelas do Identity
CREATE TABLE IF NOT EXISTS `AspNetRoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` varchar(255) NOT NULL,
    `ClaimType` longtext,
    `ClaimValue` longtext,
    PRIMARY KEY (`Id`),
    KEY `IX_AspNetRoleClaims_RoleId` (`RoleId`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AspNetUserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(255) NOT NULL,
    `ClaimType` longtext,
    `ClaimValue` longtext,
    PRIMARY KEY (`Id`),
    KEY `IX_AspNetUserClaims_UserId` (`UserId`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AspNetUserLogins` (
    `LoginProvider` varchar(255) NOT NULL,
    `ProviderKey` varchar(255) NOT NULL,
    `ProviderDisplayName` longtext,
    `UserId` varchar(255) NOT NULL,
    PRIMARY KEY (`LoginProvider`,`ProviderKey`),
    KEY `IX_AspNetUserLogins_UserId` (`UserId`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AspNetUserRoles` (
    `UserId` varchar(255) NOT NULL,
    `RoleId` varchar(255) NOT NULL,
    PRIMARY KEY (`UserId`,`RoleId`),
    KEY `IX_AspNetUserRoles_RoleId` (`RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `AspNetUserTokens` (
    `UserId` varchar(255) NOT NULL,
    `LoginProvider` varchar(255) NOT NULL,
    `Name` varchar(255) NOT NULL,
    `Value` longtext,
    PRIMARY KEY (`UserId`,`LoginProvider`,`Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Registrar migration
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260125041151_InitialCreate', '9.0.1');

-- Inserir usuário de teste
INSERT IGNORE INTO `AspNetUsers` 
    (`Id`, `UserName`, `NormalizedUserName`, `Email`, `NormalizedEmail`, `EmailConfirmed`, 
     `CreatedAt`, `IsActive`, `SecurityStamp`, `PasswordHash`, `PhoneNumberConfirmed`, 
     `TwoFactorEnabled`, `LockoutEnabled`, `AccessFailedCount`)
VALUES 
    (UUID(), 'testuser', 'TESTUSER', 'test@chatminimal.com', 'TEST@CHATMINIMAL.COM', 1,
     NOW(), 1, UUID(), NULL, 0, 0, 0, 0);

-- Inserir API Key de teste (válida por 2 anos)
SET @userId = (SELECT Id FROM AspNetUsers WHERE Email = 'test@chatminimal.com' LIMIT 1);

INSERT IGNORE INTO `ApiKeys` 
    (`Id`, `Key`, `Name`, `CreatedAt`, `ExpiresAt`, `IsActive`, `UserId`)
VALUES 
    (UUID(), 'api-key-12345678901234567890123456789012', 'Test API Key', 
     NOW(), DATE_ADD(NOW(), INTERVAL 2 YEAR), 1, @userId);

SELECT '✅ Database initialized successfully!' AS Status;
SELECT CONCAT('API Key: api-key-12345678901234567890123456789012') AS Info;
SELECT CONCAT('User Email: test@chatminimal.com') AS UserInfo;
