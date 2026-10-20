-- =====================================================================
-- Smart Task & Collaboration Engine - Database Schema
-- Run this in SSMS against your DESKTOP-E46P7NN\SQLEXPRESS instance
-- if you prefer creating the tables by hand instead of EF Core migrations.
-- =====================================================================

IF DB_ID('SmartTaskEngineDb') IS NULL
BEGIN
    CREATE DATABASE SmartTaskEngineDb;
END
GO

USE SmartTaskEngineDb;
GO

-- 1) Users -------------------------------------------------------------
-- Drop in dependency order so re-running this script is safe
IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL DROP TABLE dbo.Notifications;
IF OBJECT_ID('dbo.TaskBookmarks', 'U') IS NOT NULL DROP TABLE dbo.TaskBookmarks;
IF OBJECT_ID('dbo.ChecklistItems', 'U') IS NOT NULL DROP TABLE dbo.ChecklistItems;
IF OBJECT_ID('dbo.TaskLabels', 'U') IS NOT NULL DROP TABLE dbo.TaskLabels;
IF OBJECT_ID('dbo.Labels', 'U') IS NOT NULL DROP TABLE dbo.Labels;
IF OBJECT_ID('dbo.TaskComments', 'U') IS NOT NULL DROP TABLE dbo.TaskComments;
IF OBJECT_ID('dbo.ActivityLogs', 'U') IS NOT NULL DROP TABLE dbo.ActivityLogs;
IF OBJECT_ID('dbo.ProjectMembers', 'U') IS NOT NULL DROP TABLE dbo.ProjectMembers;
IF OBJECT_ID('dbo.TodoTasks', 'U') IS NOT NULL DROP TABLE dbo.TodoTasks;
IF OBJECT_ID('dbo.Projects', 'U') IS NOT NULL DROP TABLE dbo.Projects;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Username      NVARCHAR(100)  NOT NULL,
    Email         NVARCHAR(150)  NOT NULL,
    PasswordHash  NVARCHAR(MAX)  NOT NULL,
    Role          NVARCHAR(20)   NOT NULL DEFAULT 'Member',   -- Member | Admin
    CreatedAt     DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email    UNIQUE (Email)
);
GO

-- 2) Projects ------------------------------------------------------------
CREATE TABLE dbo.Projects (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(150)  NOT NULL,
    Description     NVARCHAR(2000) NULL,
    CreatedAt       DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedByUserId INT            NOT NULL,
    CONSTRAINT FK_Projects_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(Id)
);
GO

CREATE TABLE dbo.ProjectMembers (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT       NOT NULL,
    UserId    INT       NOT NULL,
    JoinedAt  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ProjectMembers_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProjectMembers_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT UQ_ProjectMembers_Project_User UNIQUE (ProjectId, UserId)
);
GO

-- 3) TodoTasks -----------------------------------------------------------
CREATE TABLE dbo.TodoTasks (
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    Title             NVARCHAR(200)  NOT NULL,
    Description       NVARCHAR(2000) NULL,
    Status            NVARCHAR(20)   NOT NULL DEFAULT 'ToDo',    -- ToDo | InProgress | Done
    Priority          NVARCHAR(20)   NOT NULL DEFAULT 'Medium',  -- Low | Medium | High
    DueDate           DATETIME2      NULL,
    CreatedAt         DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedByUserId   INT            NOT NULL,
    AssignedToUserId  INT            NULL,
    ProjectId         INT            NULL,
    CONSTRAINT FK_TodoTasks_CreatedBy  FOREIGN KEY (CreatedByUserId)  REFERENCES dbo.Users(Id),
    CONSTRAINT FK_TodoTasks_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_TodoTasks_Project    FOREIGN KEY (ProjectId)        REFERENCES dbo.Projects(Id) ON DELETE SET NULL
);
GO

-- 4) ActivityLogs --------------------------------------------------------
CREATE TABLE dbo.ActivityLogs (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    TaskId    INT           NOT NULL,
    UserId    INT           NOT NULL,
    Action    NVARCHAR(500) NOT NULL,
    Timestamp DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ActivityLogs_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTasks(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ActivityLogs_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

-- Helpful indexes for filtering/searching tasks
CREATE INDEX IX_TodoTasks_Status   ON dbo.TodoTasks(Status);
CREATE INDEX IX_TodoTasks_Priority ON dbo.TodoTasks(Priority);
CREATE INDEX IX_TodoTasks_ProjectId ON dbo.TodoTasks(ProjectId);
CREATE INDEX IX_ActivityLogs_TaskId ON dbo.ActivityLogs(TaskId);
GO

-- 5) Comments, labels, checklist, bookmarks, notifications --------------
CREATE TABLE dbo.TaskComments (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    TaskId    INT            NOT NULL,
    UserId    INT            NOT NULL,
    Body      NVARCHAR(2000) NOT NULL,
    CreatedAt DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_TaskComments_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTasks(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TaskComments_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

CREATE TABLE dbo.Labels (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(50) NOT NULL,
    Color           NVARCHAR(20) NOT NULL DEFAULT '#64748b',
    CreatedAt       DATETIME2    NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedByUserId INT          NOT NULL,
    CONSTRAINT FK_Labels_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(Id),
    CONSTRAINT UQ_Labels_User_Name UNIQUE (CreatedByUserId, Name)
);
GO

CREATE TABLE dbo.TaskLabels (
    TaskId  INT NOT NULL,
    LabelId INT NOT NULL,
    CONSTRAINT PK_TaskLabels PRIMARY KEY (TaskId, LabelId),
    CONSTRAINT FK_TaskLabels_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTasks(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TaskLabels_Label FOREIGN KEY (LabelId) REFERENCES dbo.Labels(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.ChecklistItems (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    TaskId    INT           NOT NULL,
    Title     NVARCHAR(200) NOT NULL,
    IsDone    BIT           NOT NULL DEFAULT 0,
    SortOrder INT           NOT NULL DEFAULT 0,
    CreatedAt DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ChecklistItems_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTasks(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.TaskBookmarks (
    UserId    INT       NOT NULL,
    TaskId    INT       NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_TaskBookmarks PRIMARY KEY (UserId, TaskId),
    CONSTRAINT FK_TaskBookmarks_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TaskBookmarks_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTasks(Id) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Notifications (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT           NOT NULL,
    ActorUserId INT           NULL,
    TaskId      INT           NULL,
    ProjectId   INT           NULL,
    Type        NVARCHAR(30)  NOT NULL,
    Message     NVARCHAR(300) NOT NULL,
    IsRead      BIT           NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Notifications_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Notifications_Actor FOREIGN KEY (ActorUserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Notifications_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTasks(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Notifications_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE SET NULL
);
GO
