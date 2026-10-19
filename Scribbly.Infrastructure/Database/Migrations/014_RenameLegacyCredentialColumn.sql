SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.GameResult', 'CredentialFingerprint') IS NOT NULL
   AND COL_LENGTH('dbo.GameResult', 'CredentialHash') IS NULL
BEGIN
    EXEC sp_rename 'dbo.GameResult.CredentialFingerprint', 'CredentialHash', 'COLUMN';
END;

IF COL_LENGTH('dbo.AccountScoreAward', 'CredentialFingerprint') IS NOT NULL
   AND COL_LENGTH('dbo.AccountScoreAward', 'CredentialHash') IS NULL
BEGIN
    EXEC sp_rename 'dbo.AccountScoreAward.CredentialFingerprint', 'CredentialHash', 'COLUMN';
END;

COMMIT;
