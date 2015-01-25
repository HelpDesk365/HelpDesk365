CREATE TABLE [dbo].[tb_user_info] (
    [guid]        UNIQUEIDENTIFIER NOT NULL,
    [email]       NVARCHAR (100)   NULL,
    [name]        NCHAR (10)       NULL,
    [company_seq] INT              NULL,
    [password]    NCHAR (100)      NULL,
    [company_nm] NCHAR(100) NULL, 
    PRIMARY KEY CLUSTERED ([guid] ASC)
);

