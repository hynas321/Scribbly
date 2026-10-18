SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.Prompt', 'U') IS NULL
    THROW 50007, 'Prompt catalog table is missing. Migration 005 must run first.', 1;

IF COL_LENGTH('dbo.Prompt', 'Category') IS NULL
    ALTER TABLE dbo.Prompt ADD Category NVARCHAR(64) NOT NULL CONSTRAINT DF_Prompt_Category DEFAULT N'general';

IF COL_LENGTH('dbo.Prompt', 'Kind') IS NULL
    ALTER TABLE dbo.Prompt ADD Kind VARCHAR(16) NOT NULL CONSTRAINT DF_Prompt_Kind DEFAULT 'word';

ALTER TABLE dbo.Prompt ALTER COLUMN Text NVARCHAR(120) NOT NULL;
ALTER TABLE dbo.Prompt ALTER COLUMN Language NVARCHAR(5) NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID('dbo.Prompt') AND name IN ('CK_Prompt_Kind', 'CK_Word_Kind'))
    ALTER TABLE dbo.Prompt ADD CONSTRAINT CK_Prompt_Kind CHECK (Kind IN ('word', 'phrase', 'saying'));

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID('dbo.Prompt') AND name IN ('CK_Prompt_Category', 'CK_Word_Category'))
    ALTER TABLE dbo.Prompt ADD CONSTRAINT CK_Prompt_Category CHECK (LEN(LTRIM(RTRIM(Category))) BETWEEN 1 AND 64);

COMMIT;
