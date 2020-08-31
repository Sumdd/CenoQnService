/*
 Navicat Premium Data Transfer

 Source Server         : _5_SQL_61.162.59.59
 Source Server Type    : SQL Server
 Source Server Version : 13001601
 Source Host           : 61.162.59.59:1433
 Source Catalog        : CcXx
 Source Schema         : dbo

 Target Server Type    : SQL Server
 Target Server Version : 13001601
 File Encoding         : 65001

 Date: 11/06/2020 15:15:56
*/


-- ----------------------------
-- Table structure for call_repair
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[call_repair]') AND type IN ('U'))
	DROP TABLE [dbo].[call_repair]
GO

CREATE TABLE [dbo].[call_repair] (
  [Id] varchar(70) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [XybaseId] varchar(70) COLLATE Chinese_PRC_CI_AS  NULL,
  [CardInfoId] varchar(70) COLLATE Chinese_PRC_CI_AS  NULL,
  [Shfzh] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [AddUserId] varchar(70) COLLATE Chinese_PRC_CI_AS  NULL,
  [AddTime] datetime DEFAULT (getdate()) NULL,
  [UpdateUserId] varchar(70) COLLATE Chinese_PRC_CI_AS  NULL,
  [UpdateTime] datetime  NULL,
  [IsDel] smallint  NULL,
  [RespMsg] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [RespState] smallint  NULL,
  [RequestID] varchar(32) COLLATE Chinese_PRC_CI_AS  NULL,
  [MD5Shfzh] varchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [ReqJson] nvarchar(max) COLLATE Chinese_PRC_CI_AS  NULL
)
GO

ALTER TABLE [dbo].[call_repair] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'Id',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'Id'
GO

EXEC sp_addextendedproperty
'MS_Description', N'共案ID',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'XybaseId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'案件ID',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'CardInfoId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'身份证',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'Shfzh'
GO

EXEC sp_addextendedproperty
'MS_Description', N'添加人',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'AddUserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'添加时间',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'AddTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'更新人',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'UpdateUserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'更新时间',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'UpdateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否假删',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'IsDel'
GO

EXEC sp_addextendedproperty
'MS_Description', N'响应结果',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'RespMsg'
GO

EXEC sp_addextendedproperty
'MS_Description', N'阶段码1.提交成功.2.提交失败.3.结果获取成功.4结果获取失败',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'RespState'
GO

EXEC sp_addextendedproperty
'MS_Description', N'唯一ID',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'RequestID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'MD5后的身份证,方便查询',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'MD5Shfzh'
GO

EXEC sp_addextendedproperty
'MS_Description', N'请求文件内容JSON',
'SCHEMA', N'dbo',
'TABLE', N'call_repair',
'COLUMN', N'ReqJson'
GO


-- ----------------------------
-- Table structure for call_repair_info
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[call_repair_info]') AND type IN ('U'))
	DROP TABLE [dbo].[call_repair_info]
GO

CREATE TABLE [dbo].[call_repair_info] (
  [Id] varchar(70) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [Xm] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [Ywy] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [Shfzh] nvarchar(50) COLLATE Chinese_PRC_CI_AS  NULL,
  [sno] varchar(70) COLLATE Chinese_PRC_CI_AS  NULL,
  [requestID] varchar(32) COLLATE Chinese_PRC_CI_AS  NULL,
  [AddUserId] varchar(70) COLLATE Chinese_PRC_CI_AS  NULL,
  [AddTime] datetime DEFAULT (getdate()) NULL,
  [UpdateUserId] varchar(70) COLLATE Chinese_PRC_CI_AS  NULL,
  [UpdateTime] datetime  NULL,
  [IsDel] smallint  NULL
)
GO

ALTER TABLE [dbo].[call_repair_info] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'Id',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_info',
'COLUMN', N'Id'
GO

EXEC sp_addextendedproperty
'MS_Description', N'姓名',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_info',
'COLUMN', N'Xm'
GO

EXEC sp_addextendedproperty
'MS_Description', N'业务员',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_info',
'COLUMN', N'Ywy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'身份证',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_info',
'COLUMN', N'Shfzh'
GO

EXEC sp_addextendedproperty
'MS_Description', N'数据唯一标识',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_info',
'COLUMN', N'sno'
GO

EXEC sp_addextendedproperty
'MS_Description', N'请求ID',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_info',
'COLUMN', N'requestID'
GO

EXEC sp_addextendedproperty
'MS_Description', N'添加人',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_info',
'COLUMN', N'AddUserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'添加时间',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_info',
'COLUMN', N'AddTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'更新人',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_info',
'COLUMN', N'UpdateUserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'更新时间',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_info',
'COLUMN', N'UpdateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否假删',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_info',
'COLUMN', N'IsDel'
GO


-- ----------------------------
-- Table structure for call_repair_list
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[dbo].[call_repair_list]') AND type IN ('U'))
	DROP TABLE [dbo].[call_repair_list]
GO

CREATE TABLE [dbo].[call_repair_list] (
  [Id] varchar(70) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [sno] varchar(70) COLLATE Chinese_PRC_CI_AS  NULL,
  [cid] varchar(70) COLLATE Chinese_PRC_CI_AS  NULL,
  [username] varchar(70) COLLATE Chinese_PRC_CI_AS  NULL,
  [tag] int  NULL,
  [extendColumn] varchar(max) COLLATE Chinese_PRC_CI_AS  NULL,
  [serialNO] varchar(64) COLLATE Chinese_PRC_CI_AS  NULL,
  [hostNum] varchar(16) COLLATE Chinese_PRC_CI_AS  NULL,
  [requestID] varchar(32) COLLATE Chinese_PRC_CI_AS  NULL,
  [AddUserId] varchar(70) COLLATE Chinese_PRC_CI_AS  NULL,
  [AddTime] datetime DEFAULT (getdate()) NULL,
  [UpdateUserId] varchar(70) COLLATE Chinese_PRC_CI_AS  NULL,
  [UpdateTime] datetime  NULL,
  [IsDel] smallint  NULL
)
GO

ALTER TABLE [dbo].[call_repair_list] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'添加人',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_list',
'COLUMN', N'AddUserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'添加时间',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_list',
'COLUMN', N'AddTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'更新人',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_list',
'COLUMN', N'UpdateUserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'更新时间',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_list',
'COLUMN', N'UpdateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否假删',
'SCHEMA', N'dbo',
'TABLE', N'call_repair_list',
'COLUMN', N'IsDel'
GO


-- ----------------------------
-- Primary Key structure for table call_repair
-- ----------------------------
ALTER TABLE [dbo].[call_repair] ADD CONSTRAINT [PK_call_repair] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table call_repair_info
-- ----------------------------
ALTER TABLE [dbo].[call_repair_info] ADD CONSTRAINT [PK_call_repair_info] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table call_repair_list
-- ----------------------------
ALTER TABLE [dbo].[call_repair_list] ADD CONSTRAINT [PK_call_repair_list] PRIMARY KEY CLUSTERED ([Id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

