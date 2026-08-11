CREATE TABLE [dbo].[user] (
    [username]                NVARCHAR (300) NOT NULL,
    [password]                NVARCHAR (300) NOT NULL,
    [email]                   NVARCHAR (300) NOT NULL,
    [password_reset_required] BIT            NOT NULL CONSTRAINT [DF_user_password_reset_required] DEFAULT (0),
    [session_version]         INT            NOT NULL CONSTRAINT [DF_user_session_version] DEFAULT (1),
    PRIMARY KEY CLUSTERED ([username] ASC)
);
