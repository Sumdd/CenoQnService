/*
Run this script on:

        61.162.59.59.CcXx    -  This database will be modified

to synchronize it with:

        61.162.59.59.Ceno_CcXx

You are recommended to back up your database before running this script

Script created by SQL Compare version 13.1.1.5299 from Red Gate Software Ltd at 2020/9/15 07:44:48

*/
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL Serializable
GO
BEGIN TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[call_repair]'
GO
ALTER TABLE [dbo].[call_repair] DROP CONSTRAINT [DF__call_repa__AddTi__239E4DCF]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Dropping constraints from [dbo].[call_repair_list]'
GO
ALTER TABLE [dbo].[call_repair_list] DROP CONSTRAINT [DF__call_repa__AddTi__47DBAE45]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering [dbo].[call_repair_info]'
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
ALTER TABLE [dbo].[call_repair_info] ADD
[username] [varchar] (50) COLLATE Chinese_PRC_CI_AS NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[call_repair_user]'
GO
CREATE TABLE [dbo].[call_repair_user]
(
[username] [varchar] (50) COLLATE Chinese_PRC_CI_AS NOT NULL,
[userpwd] [varchar] (50) COLLATE Chinese_PRC_CI_AS NOT NULL,
[usernumber] [varchar] (50) COLLATE Chinese_PRC_CI_AS NOT NULL,
[localpwd] [varchar] (50) COLLATE Chinese_PRC_CI_AS NOT NULL,
[localip] [varchar] (50) COLLATE Chinese_PRC_CI_AS NULL,
[localua] [varchar] (50) COLLATE Chinese_PRC_CI_AS NULL,
[AddUserId] [varchar] (70) COLLATE Chinese_PRC_CI_AS NULL,
[AddTime] [datetime] NULL CONSTRAINT [DF_call_user_AddTime] DEFAULT (getdate()),
[UpdateUserId] [varchar] (70) COLLATE Chinese_PRC_CI_AS NULL,
[UpdateTime] [datetime] NULL,
[IsDel] [smallint] NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_call_user] on [dbo].[call_repair_user]'
GO
ALTER TABLE [dbo].[call_repair_user] ADD CONSTRAINT [PK_call_user] PRIMARY KEY CLUSTERED  ([username])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[call_repair_record]'
GO
CREATE TABLE [dbo].[call_repair_record]
(
[sessionId] [varchar] (70) COLLATE Chinese_PRC_CI_AS NOT NULL,
[username] [varchar] (50) COLLATE Chinese_PRC_CI_AS NULL,
[userData] [nvarchar] (64) COLLATE Chinese_PRC_CI_AS NULL,
[ani] [varchar] (64) COLLATE Chinese_PRC_CI_AS NULL,
[dnis] [varchar] (64) COLLATE Chinese_PRC_CI_AS NULL,
[dani] [varchar] (64) COLLATE Chinese_PRC_CI_AS NULL,
[ddnis] [varchar] (64) COLLATE Chinese_PRC_CI_AS NULL,
[startTime] [datetime] NULL,
[endTime] [datetime] NULL,
[callResult] [tinyint] NULL,
[alertDuration] [int] NULL,
[talkDuration] [int] NULL,
[endType] [int] NULL,
[AddUserId] [varchar] (70) COLLATE Chinese_PRC_CI_AS NULL,
[AddTime] [datetime] NULL CONSTRAINT [DF_call_repair_record_AddTime] DEFAULT (getdate()),
[UpdateUserId] [varchar] (70) COLLATE Chinese_PRC_CI_AS NULL,
[UpdateTime] [datetime] NULL,
[IsDel] [smallint] NULL,
[auto_status] [tinyint] NULL,
[auto_err] [nvarchar] (50) COLLATE Chinese_PRC_CI_AS NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_call_repair_record] on [dbo].[call_repair_record]'
GO
ALTER TABLE [dbo].[call_repair_record] ADD CONSTRAINT [PK_call_repair_record] PRIMARY KEY CLUSTERED  ([sessionId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [dbo].[call_repair]'
GO
ALTER TABLE [dbo].[call_repair] ADD CONSTRAINT [DF_call_repair_AddTime] DEFAULT (getdate()) FOR [AddTime]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [dbo].[call_repair_list]'
GO
ALTER TABLE [dbo].[call_repair_list] ADD CONSTRAINT [DF_call_repair_list_AddTime] DEFAULT (getdate()) FOR [AddTime]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating extended properties'
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'坐席ID', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_info', 'COLUMN', N'username'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'添加时间', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'AddTime'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'添加人', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'AddUserId'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'振铃时长', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'alertDuration'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'主叫号码', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'ani'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'状态结果', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'auto_err'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'状态:0仅ID,1成功,2失败', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'auto_status'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'外呼结果:1成功;2失败', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'callResult'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'主叫外显号码', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'dani'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'被叫外显号码', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'ddnis'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'被叫号码', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'dnis'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'通话结束时间', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'endTime'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'挂断类型:1客户挂断;2坐席挂断;-1未知(一般为振铃挂断或外呼失败)', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'endType'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'是否假删', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'IsDel'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'通话唯一标识', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'sessionId'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'通话开始时间', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'startTime'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'通话时长', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'talkDuration'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'更新时间', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'UpdateTime'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'更新人', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'UpdateUserId'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'随路信令', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'userData'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'坐席ID', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_record', 'COLUMN', N'username'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'添加时间', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_user', 'COLUMN', N'AddTime'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'添加人', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_user', 'COLUMN', N'AddUserId'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'是否假删', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_user', 'COLUMN', N'IsDel'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'缓存本机IP', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_user', 'COLUMN', N'localip'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'缓存本机密码,默认与坐席密码一致即可', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_user', 'COLUMN', N'localpwd'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'缓存本机Ua', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_user', 'COLUMN', N'localua'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'更新时间', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_user', 'COLUMN', N'UpdateTime'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'更新人', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_user', 'COLUMN', N'UpdateUserId'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'坐席ID', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_user', 'COLUMN', N'username'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'坐席号码', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_user', 'COLUMN', N'usernumber'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
BEGIN TRY
	EXEC sp_addextendedproperty N'MS_Description', N'坐席密码', 'SCHEMA', N'dbo', 'TABLE', N'call_repair_user', 'COLUMN', N'userpwd'
END TRY
BEGIN CATCH
	DECLARE @msg nvarchar(max);
	DECLARE @severity int;
	DECLARE @state int;
	SELECT @msg = ERROR_MESSAGE(), @severity = ERROR_SEVERITY(), @state = ERROR_STATE();
	RAISERROR(@msg, @severity, @state);

	SET NOEXEC ON
END CATCH
GO
COMMIT TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
-- This statement writes to the SQL Server Log so SQL Monitor can show this deployment.
IF HAS_PERMS_BY_NAME(N'sys.xp_logevent', N'OBJECT', N'EXECUTE') = 1
BEGIN
    DECLARE @databaseName AS nvarchar(2048), @eventMessage AS nvarchar(2048)
    SET @databaseName = REPLACE(REPLACE(DB_NAME(), N'\', N'\\'), N'"', N'\"')
    SET @eventMessage = N'Redgate SQL Compare: { "deployment": { "description": "Redgate SQL Compare deployed to ' + @databaseName + N'", "database": "' + @databaseName + N'" }}'
    EXECUTE sys.xp_logevent 55000, @eventMessage
END
GO
DECLARE @Success AS BIT
SET @Success = 1
SET NOEXEC OFF
IF (@Success = 1) PRINT 'The database update succeeded'
ELSE BEGIN
	IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
	PRINT 'The database update failed'
END
GO
