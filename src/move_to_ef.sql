BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503230132_OnboardEntityFrameworkCore'
)
BEGIN
    CREATE TABLE [Speciality] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Speciality] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503230132_OnboardEntityFrameworkCore'
)
BEGIN
    CREATE TABLE [Question] (
        [Id] nvarchar(450) NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [QuestionType] nvarchar(max) NOT NULL,
        [QuestionText] nvarchar(max) NOT NULL,
        [CorrectAnswerLetter] nvarchar(max) NOT NULL,
        [Explanation] nvarchar(max) NOT NULL,
        [ClinicalTips] nvarchar(max) NOT NULL,
        [LearningPoints] nvarchar(max) NOT NULL,
        [QuestionState] nvarchar(max) NOT NULL,
        [SpecialityId] nvarchar(450) NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NOT NULL,
        [UpdatedOn] datetime2 NOT NULL,
        CONSTRAINT [PK_Question] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Question_Speciality_SpecialityId] FOREIGN KEY ([SpecialityId]) REFERENCES [Speciality] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503230132_OnboardEntityFrameworkCore'
)
BEGIN
    CREATE TABLE [AnswerOption] (
        [Id] nvarchar(450) NOT NULL,
        [Letter] nvarchar(max) NOT NULL,
        [Text] nvarchar(max) NOT NULL,
        [QuestionId] nvarchar(450) NULL,
        CONSTRAINT [PK_AnswerOption] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AnswerOption_Question_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Question] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503230132_OnboardEntityFrameworkCore'
)
BEGIN
    CREATE INDEX [IX_AnswerOption_QuestionId] ON [AnswerOption] ([QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503230132_OnboardEntityFrameworkCore'
)
BEGIN
    CREATE INDEX [IX_Question_SpecialityId] ON [Question] ([SpecialityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503230132_OnboardEntityFrameworkCore'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240503230132_OnboardEntityFrameworkCore', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240506013211_QuestionAttemptAndFlagging'
)
BEGIN
    ALTER TABLE [AnswerOption] DROP CONSTRAINT [FK_AnswerOption_Question_QuestionId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240506013211_QuestionAttemptAndFlagging'
)
BEGIN
    DROP INDEX [IX_AnswerOption_QuestionId] ON [AnswerOption];
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AnswerOption]') AND [c].[name] = N'QuestionId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [AnswerOption] DROP CONSTRAINT [' + @var0 + '];');
    EXEC(N'UPDATE [AnswerOption] SET [QuestionId] = N'''' WHERE [QuestionId] IS NULL');
    ALTER TABLE [AnswerOption] ALTER COLUMN [QuestionId] nvarchar(450) NOT NULL;
    ALTER TABLE [AnswerOption] ADD DEFAULT N'' FOR [QuestionId];
    CREATE INDEX [IX_AnswerOption_QuestionId] ON [AnswerOption] ([QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240506013211_QuestionAttemptAndFlagging'
)
BEGIN
    CREATE TABLE [FlaggedQuestion] (
        [Id] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [QuestionId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_FlaggedQuestion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FlaggedQuestion_Question_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Question] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240506013211_QuestionAttemptAndFlagging'
)
BEGIN
    CREATE TABLE [QuestionAttempt] (
        [Id] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [QuestionId] nvarchar(450) NOT NULL,
        [ChosenAnswer] nvarchar(max) NOT NULL,
        [CorrectAnswer] nvarchar(max) NOT NULL,
        [IsCorrect] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        CONSTRAINT [PK_QuestionAttempt] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QuestionAttempt_Question_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Question] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240506013211_QuestionAttemptAndFlagging'
)
BEGIN
    CREATE INDEX [IX_FlaggedQuestion_QuestionId] ON [FlaggedQuestion] ([QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240506013211_QuestionAttemptAndFlagging'
)
BEGIN
    CREATE INDEX [IX_QuestionAttempt_QuestionId] ON [QuestionAttempt] ([QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240506013211_QuestionAttemptAndFlagging'
)
BEGIN
    ALTER TABLE [AnswerOption] ADD CONSTRAINT [FK_AnswerOption_Question_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Question] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240506013211_QuestionAttemptAndFlagging'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240506013211_QuestionAttemptAndFlagging', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511011431_AnswerOptions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240511011431_AnswerOptions', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511012450_FlaggedQuestionsCreatedDate'
)
BEGIN
    ALTER TABLE [FlaggedQuestion] ADD [CreatedOn] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511012450_FlaggedQuestionsCreatedDate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240511012450_FlaggedQuestionsCreatedDate', N'8.0.0');
END;
GO

COMMIT;
GO

