IF COL_LENGTH('dbo.GameResult', 'Username') IS NULL\nBEGIN\n    ALTER TABLE dbo.GameResult ADD Username NVARCHAR(255) NULL;\nEND;\n
