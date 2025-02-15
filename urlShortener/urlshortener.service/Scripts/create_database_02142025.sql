IF DB_ID('urlshortener_db') IS NULL
BEGIN
    CREATE DATABASE urlshortener_db;
END;
GO

USE urlshortener_db;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [UrlMappings] (
    [Id] bigint NOT NULL IDENTITY,
    [LongUrl] nvarchar(450) NOT NULL,
    [ShortUrl] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_UrlMappings] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_UrlMappings_LongUrl] ON [UrlMappings] ([LongUrl]);

CREATE INDEX [IX_UrlMappings_ShortUrl] ON [UrlMappings] ([ShortUrl]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250125185732_CreateInitialDb', N'9.0.1');

ALTER TABLE [UrlMappings] DROP CONSTRAINT [PK_UrlMappings];

DROP INDEX [IX_UrlMappings_LongUrl] ON [UrlMappings];

ALTER TABLE [UrlMappings] ADD CONSTRAINT [PK_UrlMappings] PRIMARY KEY ([LongUrl]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250125192423_UpdateIndexesAndKey', N'9.0.1');

COMMIT;
GO


