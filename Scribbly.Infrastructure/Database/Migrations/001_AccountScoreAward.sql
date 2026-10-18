IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AccountScoreAward')
BEGIN
    CREATE TABLE AccountScoreAward (
        GameHash NVARCHAR(100) NOT NULL,
        PlayerToken NVARCHAR(255) NOT NULL,
        AccessToken NVARCHAR(MAX) NOT NULL,
        Score INT NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_AccountScoreAward PRIMARY KEY (GameHash, PlayerToken)
    );
END;
