CREATE TABLE PasswordResetTokens(
    UserId INT NOT NULL,
    Token NVARCHAR(100) NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    FOREIGN KEY(UserId) REFERENCES Users(Id)
);
