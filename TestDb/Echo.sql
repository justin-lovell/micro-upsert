CREATE PROCEDURE [dbo].Echo
	@Msg nvarchar(max)
AS
	set nocount on
	select @Msg

