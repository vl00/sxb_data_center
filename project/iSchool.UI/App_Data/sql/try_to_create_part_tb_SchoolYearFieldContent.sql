--- replace SchoolYearFieldContent_XXX
--USE iSchoolData

IF OBJECT_ID(N'dbo.SchoolYearFieldContent_{0}',N'U') is null 
BEGIN
-- table
CREATE TABLE [dbo].[SchoolYearFieldContent_{0}]
(
	[Id] [uniqueidentifier] NOT NULL,
	[year] [int] NOT NULL,
	[eid] [uniqueidentifier] NOT NULL,
	[field] [varchar](20) NULL,
	[content] [nvarchar](max) NULL,
	[IsValid] [bit] NOT NULL,
 CONSTRAINT [PK_SchoolYearFieldContent_{0}] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
;;
ALTER TABLE [dbo].[SchoolYearFieldContent_{0}] ADD  CONSTRAINT [DF_SchoolYearFieldContent_{0}_IsValid]  DEFAULT ((1)) FOR [IsValid]
;;

-- idx
CREATE NONCLUSTERED INDEX [IX_eid] ON [dbo].[SchoolYearFieldContent_{0}]
(
	[eid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;;
CREATE NONCLUSTERED INDEX [IX_field] ON [dbo].[SchoolYearFieldContent_{0}]
(
	[field] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;;
CREATE NONCLUSTERED INDEX [IX_IsValid] ON [dbo].[SchoolYearFieldContent_{0}]
(
	[IsValid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;;
/****** Object:  Index [IX_eid_field_IsValid]    Script Date: 2020/7/14 10:56:36 ******/
CREATE NONCLUSTERED INDEX [IX_eid_field_IsValid] ON [dbo].[SchoolYearFieldContent_{0}]
(
	[eid] ASC,
	[field] ASC,
	[IsValid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;;
END

IF OBJECT_ID(N'dbo.OnlineSchoolYearFieldContent_{0}',N'U') is null 
BEGIN
-- table
CREATE TABLE [dbo].[OnlineSchoolYearFieldContent_{0}]
(
	[Id] [uniqueidentifier] NOT NULL,
	[year] [int] NOT NULL,
	[eid] [uniqueidentifier] NOT NULL,
	[field] [varchar](20) NULL,
	[content] [nvarchar](max) NULL,
	[IsValid] [bit] NOT NULL,
 CONSTRAINT [PK_OnlineSchoolYearFieldContent_{0}] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
;;
ALTER TABLE [dbo].[OnlineSchoolYearFieldContent_{0}] ADD  CONSTRAINT [DF_OnlineSchoolYearFieldContent_{0}_IsValid]  DEFAULT ((1)) FOR [IsValid]
;;

-- idx
CREATE NONCLUSTERED INDEX [IX_eid] ON [dbo].[OnlineSchoolYearFieldContent_{0}]
(
	[eid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;;
CREATE NONCLUSTERED INDEX [IX_field] ON [dbo].[OnlineSchoolYearFieldContent_{0}]
(
	[field] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;;
CREATE NONCLUSTERED INDEX [IX_IsValid] ON [dbo].[OnlineSchoolYearFieldContent_{0}]
(
	[IsValid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;;
/****** Object:  Index [IX_eid_field_IsValid]    Script Date: 2020/7/14 10:56:36 ******/
CREATE NONCLUSTERED INDEX [IX_eid_field_IsValid] ON [dbo].[OnlineSchoolYearFieldContent_{0}]
(
	[eid] ASC,
	[field] ASC,
	[IsValid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;;
END
