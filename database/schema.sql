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
IF OBJECT_ID('dbo.ActivityLogs', 'U') IS NOT NULL DROP TABLE dbo.ActivityLogs;
IF OBJECT_ID('dbo.TodoTasks', 'U') IS NOT NULL DROP TABLE dbo.TodoTasks;
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

-- 2) TodoTasks -----------------------------------------------------------
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
    CONSTRAINT FK_TodoTasks_CreatedBy  FOREIGN KEY (CreatedByUserId)  REFERENCES dbo.Users(Id),
    CONSTRAINT FK_TodoTasks_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES dbo.Users(Id)
);
GO

-- 3) ActivityLogs --------------------------------------------------------
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
CREATE INDEX IX_ActivityLogs_TaskId ON dbo.ActivityLogs(TaskId);
GO
