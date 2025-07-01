IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'MyDatabase')
BEGIN
    CREATE DATABASE MyDatabase;
END
GO

USE MyDatabase;
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Word')
BEGIN
    CREATE TABLE Word (
        Text NVARCHAR(255),
        Language NVARCHAR(50)
    );
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Account')
BEGIN
    CREATE TABLE Account (
        Id NVARCHAR(100) PRIMARY KEY,
        AccessToken NVARCHAR(MAX),
        Email NVARCHAR(255),
        Name NVARCHAR(255),
        GivenName NVARCHAR(255),
        FamilyName NVARCHAR(255),
        Score INT
    );
END
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM Word)
BEGIN
    INSERT INTO Word (Text, Language) VALUES
    ('apple', 'en'), (N'jabłko', 'pl'),
    ('ball', 'en'), (N'piłka', 'pl'),
    ('book', 'en'), (N'książka', 'pl'),
    ('chair', 'en'), (N'krzesło', 'pl'),
    ('lamp', 'en'), (N'lampa', 'pl'),
    ('fan', 'en'), (N'wentylator', 'pl'),
    ('shirt', 'en'), (N'koszula', 'pl'),
    ('pants', 'en'), (N'spodnie', 'pl'),
    ('glasses', 'en'), (N'okulary', 'pl'),
    ('wallet', 'en'), (N'portfel', 'pl'),
    ('blanket', 'en'), (N'koc', 'pl'),
    ('shovel', 'en'), (N'łopata', 'pl'),
    ('ladder', 'en'), (N'drabina', 'pl'),
    ('hammer', 'en'), (N'młotek', 'pl'),
    ('key', 'en'), (N'klucz', 'pl'),
    ('rope', 'en'), (N'lina', 'pl'),
    ('watch', 'en'), (N'zegarek', 'pl'),
    ('telescope', 'en'), (N'teleskop', 'pl'),
    ('umbrella', 'en'), (N'parasol', 'pl'),
    ('pencil', 'en'), (N'ołówek', 'pl'),
    ('pen', 'en'), (N'długopis', 'pl'),
    ('scissors', 'en'), (N'nożyczki', 'pl'),
    ('paper', 'en'), (N'papier', 'pl'),
    ('bottle', 'en'), (N'butelka', 'pl'),
    ('bucket', 'en'), (N'wiadro', 'pl'),
    ('candle', 'en'), (N'świeca', 'pl'),
    ('pillow', 'en'), (N'poduszka', 'pl'),
    ('spoon', 'en'), (N'łyżka', 'pl'),
    ('fork', 'en'), (N'widelec', 'pl'),
    ('knife', 'en'), (N'nóż', 'pl'),
    ('cup', 'en'), (N'kubek', 'pl'),
    ('broom', 'en'), (N'miotła', 'pl'),
    ('nail', 'en'), (N'gwóźdź', 'pl'),
    ('window', 'en'), (N'okno', 'pl'),
    ('door', 'en'), (N'drzwi', 'pl'),
    ('television', 'en'), (N'telewizor', 'pl'),
    ('camera', 'en'), (N'aparat', 'pl'),
    ('computer', 'en'), (N'komputer', 'pl'),
    ('remote', 'en'), (N'pilot', 'pl'),
    ('couch', 'en'), (N'kanapa', 'pl'),
    ('carpet', 'en'), (N'dywan', 'pl'),
    ('shelf', 'en'), (N'półka', 'pl'),
    ('mirror', 'en'), (N'lustro', 'pl'),
    ('picture', 'en'), (N'obraz', 'pl'),
    ('clock', 'en'), (N'zegar', 'pl'),
    ('toothbrush', 'en'), (N'szczoteczka', 'pl'),
    ('soap', 'en'), (N'mydło', 'pl'),
    ('toothpaste', 'en'), (N'pasta', 'pl'),
    ('shampoo', 'en'), (N'szampon', 'pl'),
    ('towel', 'en'), (N'ręcznik', 'pl'),
    ('bicycle', 'en'), (N'rower', 'pl'),
    ('calculator', 'en'), (N'kalkulator', 'pl'),
    ('mouse', 'en'), (N'mysz', 'pl'),
    ('keyboard', 'en'), (N'klawiatura', 'pl'),
    ('charger', 'en'), (N'ładowarka', 'pl'),
    ('tablet', 'en'), (N'tablet', 'pl'),
    ('notebook', 'en'), (N'zeszyt', 'pl'),
    ('radio', 'en'), (N'radio', 'pl');
END
GO