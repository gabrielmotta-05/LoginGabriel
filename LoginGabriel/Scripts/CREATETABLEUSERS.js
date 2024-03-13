CREATE TABLE Users(
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(MAX) NOT NULL,
    Password NVARCHAR(MAX) NOT NULL,
    ResetPasswordToken NVARCHAR(100),
    ResetPasswordTokenExpiry DATETIME,
    PlainPassword NVARCHAR(200)
);