using Microsoft.EntityFrameworkCore;

namespace SmartTaskEngine.Data
{
    public static class DatabaseBootstrap
    {
        public static async Task ApplyAsync(ApplicationDbContext db)
        {
            // Additive only: existing Users / TodoTasks / ActivityLogs stay in place.
            const string sql = """
                IF OBJECT_ID(N'dbo.Projects', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Projects (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(150) NOT NULL,
                        Description NVARCHAR(2000) NULL,
                        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Projects_CreatedAt DEFAULT SYSUTCDATETIME(),
                        CreatedByUserId INT NOT NULL,
                        CONSTRAINT FK_Projects_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(Id)
                    );
                END

                IF OBJECT_ID(N'dbo.ProjectMembers', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.ProjectMembers (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        ProjectId INT NOT NULL,
                        UserId INT NOT NULL,
                        JoinedAt DATETIME2 NOT NULL CONSTRAINT DF_ProjectMembers_JoinedAt DEFAULT SYSUTCDATETIME(),
                        CONSTRAINT FK_ProjectMembers_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE,
                        CONSTRAINT FK_ProjectMembers_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
                        CONSTRAINT UQ_ProjectMembers_Project_User UNIQUE (ProjectId, UserId)
                    );
                END

                IF COL_LENGTH(N'dbo.TodoTasks', N'ProjectId') IS NULL
                    ALTER TABLE dbo.TodoTasks ADD ProjectId INT NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TodoTasks_Project')
                    ALTER TABLE dbo.TodoTasks ADD CONSTRAINT FK_TodoTasks_Project
                        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE SET NULL;

                IF OBJECT_ID(N'dbo.TaskComments', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.TaskComments (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        TaskId INT NOT NULL,
                        UserId INT NOT NULL,
                        Body NVARCHAR(2000) NOT NULL,
                        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_TaskComments_CreatedAt DEFAULT SYSUTCDATETIME(),
                        CONSTRAINT FK_TaskComments_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTasks(Id) ON DELETE CASCADE,
                        CONSTRAINT FK_TaskComments_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
                    );
                END

                IF OBJECT_ID(N'dbo.Labels', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Labels (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(50) NOT NULL,
                        Color NVARCHAR(20) NOT NULL CONSTRAINT DF_Labels_Color DEFAULT N'#64748b',
                        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Labels_CreatedAt DEFAULT SYSUTCDATETIME(),
                        CreatedByUserId INT NOT NULL,
                        CONSTRAINT FK_Labels_CreatedBy FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users(Id),
                        CONSTRAINT UQ_Labels_User_Name UNIQUE (CreatedByUserId, Name)
                    );
                END

                IF OBJECT_ID(N'dbo.TaskLabels', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.TaskLabels (
                        TaskId INT NOT NULL,
                        LabelId INT NOT NULL,
                        CONSTRAINT PK_TaskLabels PRIMARY KEY (TaskId, LabelId),
                        CONSTRAINT FK_TaskLabels_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTasks(Id) ON DELETE CASCADE,
                        CONSTRAINT FK_TaskLabels_Label FOREIGN KEY (LabelId) REFERENCES dbo.Labels(Id) ON DELETE CASCADE
                    );
                END

                IF OBJECT_ID(N'dbo.ChecklistItems', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.ChecklistItems (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        TaskId INT NOT NULL,
                        Title NVARCHAR(200) NOT NULL,
                        IsDone BIT NOT NULL CONSTRAINT DF_ChecklistItems_IsDone DEFAULT 0,
                        SortOrder INT NOT NULL CONSTRAINT DF_ChecklistItems_SortOrder DEFAULT 0,
                        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ChecklistItems_CreatedAt DEFAULT SYSUTCDATETIME(),
                        CONSTRAINT FK_ChecklistItems_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTasks(Id) ON DELETE CASCADE
                    );
                END

                IF OBJECT_ID(N'dbo.TaskBookmarks', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.TaskBookmarks (
                        UserId INT NOT NULL,
                        TaskId INT NOT NULL,
                        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_TaskBookmarks_CreatedAt DEFAULT SYSUTCDATETIME(),
                        CONSTRAINT PK_TaskBookmarks PRIMARY KEY (UserId, TaskId),
                        CONSTRAINT FK_TaskBookmarks_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
                        CONSTRAINT FK_TaskBookmarks_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTasks(Id) ON DELETE CASCADE
                    );
                END

                IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Notifications (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        ActorUserId INT NULL,
                        TaskId INT NULL,
                        ProjectId INT NULL,
                        Type NVARCHAR(30) NOT NULL,
                        Message NVARCHAR(300) NOT NULL,
                        IsRead BIT NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT 0,
                        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT SYSUTCDATETIME(),
                        CONSTRAINT FK_Notifications_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
                        CONSTRAINT FK_Notifications_Actor FOREIGN KEY (ActorUserId) REFERENCES dbo.Users(Id),
                        CONSTRAINT FK_Notifications_Task FOREIGN KEY (TaskId) REFERENCES dbo.TodoTasks(Id) ON DELETE SET NULL,
                        CONSTRAINT FK_Notifications_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Projects(Id) ON DELETE SET NULL
                    );
                END
                """;

            await db.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
