IF COL_LENGTH('dbo.Account', 'CredentialHash') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Account DROP COLUMN CredentialHash;
END;
