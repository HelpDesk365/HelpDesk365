CREATE TABLE [dbo].[Table]
(
	[guid] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [email] NVARCHAR(100) NULL, 
    [name] NCHAR(10) NULL, 
    [company_seq] INT NULL, 
    [password] NCHAR(100) NULL
)
