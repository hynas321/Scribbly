SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.Account', 'U') IS NOT NULL
BEGIN
    UPDATE dbo.Account SET Score = 0 WHERE Score IS NULL;
    ALTER TABLE dbo.Account ALTER COLUMN Score INT NOT NULL;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.default_constraints dc
        JOIN sys.columns c ON c.default_object_id = dc.object_id
        WHERE c.object_id = OBJECT_ID('dbo.Account') AND c.name = 'Score'
    )
        ALTER TABLE dbo.Account ADD CONSTRAINT DF_Account_Score DEFAULT 0 FOR Score;

    IF COL_LENGTH('dbo.Account', 'GivenName') IS NOT NULL
        ALTER TABLE dbo.Account DROP COLUMN GivenName;
    IF COL_LENGTH('dbo.Account', 'FamilyName') IS NOT NULL
        ALTER TABLE dbo.Account DROP COLUMN FamilyName;
END;

COMMIT;
