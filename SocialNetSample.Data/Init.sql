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
GO

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Email] nvarchar(50) NOT NULL,
    [Deleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [BlockedUsers] (
    [Id] uniqueidentifier NOT NULL,
    [SourceId] uniqueidentifier NOT NULL,
    [TargetId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
    [UserIdA] AS case when SourceId < TargetId then SourceId else TargetId end,
    [UserIdB] AS case when SourceId < TargetId then TargetId else SourceId end,
    CONSTRAINT [PK_BlockedUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [BlockedUsers_SourceId_neq_TargetId] CHECK ([SourceId] != [TargetId]),
    CONSTRAINT [FK_BlockedUsers_Users_SourceId] FOREIGN KEY ([SourceId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_BlockedUsers_Users_TargetId] FOREIGN KEY ([TargetId]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [FriendRequests] (
    [Id] uniqueidentifier NOT NULL,
    [SourceId] uniqueidentifier NOT NULL,
    [TargetId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
    [UserIdA] AS case when SourceId < TargetId then SourceId else TargetId end,
    [UserIdB] AS case when SourceId < TargetId then TargetId else SourceId end,
    CONSTRAINT [PK_FriendRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FriendRequests_SourceId_neq_TargetId] CHECK ([SourceId] != [TargetId]),
    CONSTRAINT [FK_FriendRequests_Users_SourceId] FOREIGN KEY ([SourceId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_FriendRequests_Users_TargetId] FOREIGN KEY ([TargetId]) REFERENCES [Users] ([Id])
);
GO

CREATE TABLE [Friends] (
    [Id] uniqueidentifier NOT NULL,
    [SourceId] uniqueidentifier NOT NULL,
    [TargetId] uniqueidentifier NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
    [UserIdA] AS case when SourceId < TargetId then SourceId else TargetId end,
    [UserIdB] AS case when SourceId < TargetId then TargetId else SourceId end,
    CONSTRAINT [PK_Friends] PRIMARY KEY ([Id]),
    CONSTRAINT [Friends_SourceId_neq_TargetId] CHECK ([SourceId] != [TargetId]),
    CONSTRAINT [FK_Friends_Users_SourceId] FOREIGN KEY ([SourceId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_Friends_Users_TargetId] FOREIGN KEY ([TargetId]) REFERENCES [Users] ([Id])
);
GO

CREATE INDEX [IX_BlockedUsers_SourceId] ON [BlockedUsers] ([SourceId]);
GO

CREATE INDEX [IX_BlockedUsers_TargetId] ON [BlockedUsers] ([TargetId]);
GO

CREATE UNIQUE INDEX [IX_BlockedUsers_UserIdA_UserIdB] ON [BlockedUsers] ([UserIdA], [UserIdB]);
GO

CREATE INDEX [IX_FriendRequests_SourceId] ON [FriendRequests] ([SourceId]);
GO

CREATE INDEX [IX_FriendRequests_TargetId] ON [FriendRequests] ([TargetId]);
GO

CREATE UNIQUE INDEX [IX_FriendRequests_UserIdA_UserIdB] ON [FriendRequests] ([UserIdA], [UserIdB]);
GO

CREATE INDEX [IX_Friends_SourceId] ON [Friends] ([SourceId]);
GO

CREATE INDEX [IX_Friends_TargetId] ON [Friends] ([TargetId]);
GO

CREATE UNIQUE INDEX [IX_Friends_UserIdA_UserIdB] ON [Friends] ([UserIdA], [UserIdB]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220420093828_Init', N'6.0.4');
GO

COMMIT;
GO

