CREATE TABLE Users(
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    Password VARCHAR(255) NOT NULL,
    ResetPasswordToken VARCHAR(255),
    ResetPasswordTokenExpiry DATETIME
);
