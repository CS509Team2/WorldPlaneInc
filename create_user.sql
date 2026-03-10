CREATE TABLE IF NOT EXISTS `users` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Username` varchar(255) NOT NULL,
    `Password` varchar(255) NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `Username` (`Username`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci;

INSERT INTO users VALUES (null, 'user1', '1111');