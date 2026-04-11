CREATE TABLE IF NOT EXISTS `seats` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `FlightNumber` VARCHAR(20) NOT NULL,
    `Airline` VARCHAR(50) NOT NULL,
    `SeatNumber` VARCHAR(5) NOT NULL,
    `SeatClass` ENUM('Economy', 'Business', 'First') NOT NULL DEFAULT 'Economy',
    `IsAvailable` BOOLEAN NOT NULL DEFAULT TRUE,
    `Price` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `uq_flight_seat` (`FlightNumber`, `Airline`, `SeatNumber`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `bookings` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Username` VARCHAR(255) NOT NULL,
    `FlightNumber` VARCHAR(20) NOT NULL,
    `Airline` VARCHAR(50) NOT NULL,
    `SeatNumber` VARCHAR(5) NOT NULL,
    `BookedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`),
    FOREIGN KEY (`Username`) REFERENCES `users`(`Username`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci;

CREATE INDEX idx_seats_flight ON seats (FlightNumber, Airline);
CREATE INDEX idx_bookings_user ON bookings (Username);
