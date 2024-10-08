CREATE TABLE [dbo].[user] (
    [username] NVARCHAR (300) NOT NULL,
    [password] NVARCHAR (300) NOT NULL,
    [email]    NVARCHAR (300) NOT NULL,
    PRIMARY KEY CLUSTERED ([username] ASC)
);