CREATE TABLE [dbo].[riddle] (
    [riddle_id]   INT            IDENTITY (1, 1) NOT NULL,
    [riddle_text] NVARCHAR (MAX) NOT NULL,
	[riddle_hint] NVARCHAR (100),
    [answer]      NVARCHAR (100) NOT NULL,
    [username]    NVARCHAR (300) NOT NULL,
    PRIMARY KEY CLUSTERED ([riddle_id] ASC),
    CONSTRAINT [FK_UserRiddle] FOREIGN KEY ([username]) REFERENCES [dbo].[user] ([username])
);

