CREATE TABLE IF NOT EXISTS `Messages` (
    `Id` char(36) NOT NULL,
    `ConversationId` varchar(100) NOT NULL,
    `Role` varchar(20) NOT NULL,
    `Content` LONGTEXT NOT NULL,
    `Timestamp` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_Messages_ConversationId` (`ConversationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
