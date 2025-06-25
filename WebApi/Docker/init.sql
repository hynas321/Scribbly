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
    ('apple', 'en'), ('jabłko', 'pl'),
    ('ball', 'en'), ('piłka', 'pl'),
    ('book', 'en'), ('książka', 'pl'),
    ('chair', 'en'), ('krzesło', 'pl'),
    ('lamp', 'en'), ('lampa', 'pl'),
    ('fan', 'en'), ('wentylator', 'pl'),
    ('shirt', 'en'), ('koszula', 'pl'),
    ('pants', 'en'), ('spodnie', 'pl'),
    ('glasses', 'en'), ('okulary', 'pl'),
    ('wallet', 'en'), ('portfel', 'pl'),
    ('blanket', 'en'), ('koc', 'pl'),
    ('shovel', 'en'), ('łopata', 'pl'),
    ('ladder', 'en'), ('drabina', 'pl'),
    ('hammer', 'en'), ('młotek', 'pl'),
    ('key', 'en'), ('klucz', 'pl'),
    ('rope', 'en'), ('lina', 'pl'),
    ('watch', 'en'), ('zegarek', 'pl'),
    ('telescope', 'en'), ('teleskop', 'pl'),
    ('umbrella', 'en'), ('parasol', 'pl'),
    ('pencil', 'en'), ('ołówek', 'pl'),
    ('pen', 'en'), ('długopis', 'pl'),
    ('scissors', 'en'), ('nożyczki', 'pl'),
    ('paper', 'en'), ('papier', 'pl'),
    ('bottle', 'en'), ('butelka', 'pl'),
    ('bucket', 'en'), ('wiadro', 'pl'),
    ('candle', 'en'), ('świeca', 'pl'),
    ('pillow', 'en'), ('poduszka', 'pl'),
    ('spoon', 'en'), ('łyżka', 'pl'),
    ('fork', 'en'), ('widelec', 'pl'),
    ('knife', 'en'), ('nóż', 'pl'),
    ('cup', 'en'), ('kubek', 'pl'),
    ('broom', 'en'), ('miotła', 'pl'),
    ('nail', 'en'), ('gwóźdź', 'pl'),
    ('window', 'en'), ('okno', 'pl'),
    ('door', 'en'), ('drzwi', 'pl'),
    ('television', 'en'), ('telewizor', 'pl'),
    ('camera', 'en'), ('aparat', 'pl'),
    ('computer', 'en'), ('komputer', 'pl'),
    ('remote', 'en'), ('pilot', 'pl'),
    ('couch', 'en'), ('kanapa', 'pl'),
    ('carpet', 'en'), ('dywan', 'pl'),
    ('shelf', 'en'), ('półka', 'pl'),
    ('mirror', 'en'), ('lustro', 'pl'),
    ('picture', 'en'), ('obraz', 'pl'),
    ('clock', 'en'), ('zegar', 'pl'),
    ('toothbrush', 'en'), ('szczoteczka', 'pl'),
    ('soap', 'en'), ('mydło', 'pl'),
    ('toothpaste', 'en'), ('pasta', 'pl'),
    ('shampoo', 'en'), ('szampon', 'pl'),
    ('towel', 'en'), ('ręcznik', 'pl'),
    ('bicycle', 'en'), ('rower', 'pl'),
    ('calculator', 'en'), ('kalkulator', 'pl'),
    ('mouse', 'en'), ('mysz', 'pl'),
    ('keyboard', 'en'), ('klawiatura', 'pl'),
    ('charger', 'en'), ('ładowarka', 'pl'),
    ('tablet', 'en'), ('tablet', 'pl'),
    ('notebook', 'en'), ('zeszyt', 'pl'),
    ('radio', 'en'), ('radio', 'pl');
END
GO