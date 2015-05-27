﻿CREATE TABLE [dbo].[Table1]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [TheGuid] UNIQUEIDENTIFIER NOT NULL, 
    [ColA] NVARCHAR(50) NOT NULL 
)

GO

CREATE INDEX [IX_Table1_Column] ON [dbo].[Table1] ([TheGuid])
