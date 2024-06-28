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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231227070355_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231227070355_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240109045150_CustomUserData'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [City] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240109045150_CustomUserData'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [DisplayName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240109045150_CustomUserData'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [FirstName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240109045150_CustomUserData'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [GraduationYear] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240109045150_CustomUserData'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [HowDidYouHearAboutUs] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240109045150_CustomUserData'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [LastName] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240109045150_CustomUserData'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [University] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240109045150_CustomUserData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240109045150_CustomUserData', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240127181854_GraduationYear'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'GraduationYear');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [GraduationYear] int NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240127181854_GraduationYear'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240127181854_GraduationYear', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240127183524_SubscribeToPromotions'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [SubscribeToPromotions] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240127183524_SubscribeToPromotions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240127183524_SubscribeToPromotions', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240218205209_Subscription'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [SubscriptionCreatedDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240218205209_Subscription'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [SubscriptionExpiryDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240218205209_Subscription'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [SubscriptionPlanId] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240218205209_Subscription'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240218205209_Subscription', N'8.0.0');
END;
GO

COMMIT;
GO

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
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AnswerOption]') AND [c].[name] = N'QuestionId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [AnswerOption] DROP CONSTRAINT [' + @var1 + '];');
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

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511195952_Notes'
)
BEGIN
    CREATE TABLE [Note] (
        [Id] nvarchar(450) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [SpecialityId] nvarchar(450) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [UpdatedOn] datetime2 NOT NULL,
        CONSTRAINT [PK_Note] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Note_Speciality_SpecialityId] FOREIGN KEY ([SpecialityId]) REFERENCES [Speciality] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511195952_Notes'
)
BEGIN
    CREATE INDEX [IX_Note_SpecialityId] ON [Note] ([SpecialityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511195952_Notes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240511195952_Notes', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512151413_QuestionAttemptUpdate'
)
BEGIN
    ALTER TABLE [QuestionAttempt] ADD [UpdatedOn] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512151413_QuestionAttemptUpdate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240512151413_QuestionAttemptUpdate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512154959_FlashcardsInitial'
)
BEGIN
    CREATE TABLE [Flashcard] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [ImageUrl] nvarchar(max) NOT NULL,
        [SpecialityId] nvarchar(450) NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [UpdatedOn] datetime2 NULL,
        CONSTRAINT [PK_Flashcard] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Flashcard_Speciality_SpecialityId] FOREIGN KEY ([SpecialityId]) REFERENCES [Speciality] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512154959_FlashcardsInitial'
)
BEGIN
    CREATE INDEX [IX_Flashcard_SpecialityId] ON [Flashcard] ([SpecialityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512154959_FlashcardsInitial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240512154959_FlashcardsInitial', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512163826_NotesAssociationUpdate'
)
BEGIN
    ALTER TABLE [Note] DROP CONSTRAINT [FK_Note_Speciality_SpecialityId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512163826_NotesAssociationUpdate'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Note]') AND [c].[name] = N'SpecialityId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Note] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Note] ALTER COLUMN [SpecialityId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512163826_NotesAssociationUpdate'
)
BEGIN
    ALTER TABLE [Note] ADD [FlashcardId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512163826_NotesAssociationUpdate'
)
BEGIN
    ALTER TABLE [Note] ADD [QuestionId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512163826_NotesAssociationUpdate'
)
BEGIN
    CREATE INDEX [IX_Note_FlashcardId] ON [Note] ([FlashcardId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512163826_NotesAssociationUpdate'
)
BEGIN
    CREATE INDEX [IX_Note_QuestionId] ON [Note] ([QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512163826_NotesAssociationUpdate'
)
BEGIN
    ALTER TABLE [Note] ADD CONSTRAINT [FK_Note_Flashcard_FlashcardId] FOREIGN KEY ([FlashcardId]) REFERENCES [Flashcard] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512163826_NotesAssociationUpdate'
)
BEGIN
    ALTER TABLE [Note] ADD CONSTRAINT [FK_Note_Question_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Question] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512163826_NotesAssociationUpdate'
)
BEGIN
    ALTER TABLE [Note] ADD CONSTRAINT [FK_Note_Speciality_SpecialityId] FOREIGN KEY ([SpecialityId]) REFERENCES [Speciality] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240512163826_NotesAssociationUpdate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240512163826_NotesAssociationUpdate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240520175125_UserNotifications'
)
BEGIN
    CREATE TABLE [UserNotification] (
        [Id] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [IsRead] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ReadOn] datetime2 NULL,
        CONSTRAINT [PK_UserNotification] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240520175125_UserNotifications'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240520175125_UserNotifications', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240520185354_MockExam'
)
BEGIN
    CREATE TABLE [MockExam] (
        [Id] nvarchar(450) NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [MockExamType] nvarchar(max) NOT NULL,
        [TotalQuestions] int NOT NULL,
        [QuestionsCompleted] int NOT NULL,
        [StartedOn] datetime2 NOT NULL,
        [CompletedOn] datetime2 NULL,
        CONSTRAINT [PK_MockExam] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240520185354_MockExam'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240520185354_MockExam', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240603222713_VideoUrl'
)
BEGIN
    ALTER TABLE [Question] ADD [VideoUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240603222713_VideoUrl'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240603222713_VideoUrl', N'8.0.0');
END;
GO

COMMIT;
GO

