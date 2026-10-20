# Smart Task & Collaboration Engine

ASP.NET Core (.NET 8) Web API — JWT auth, Task CRUD, assignment, and automatic activity logging.

## 📁 Folder Structure
```
SmartTaskEngine/
├── SmartTaskEngine.sln
├── SmartTaskEngine.csproj
├── Program.cs
├── appsettings.json              ← connection string + JWT key live here
├── appsettings.Development.json
├── database/
│   └── schema.sql                ← optional: create tables manually via SSMS
├── Controllers/
│   ├── AuthController.cs
│   └── TasksController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── User.cs
│   ├── TodoTask.cs
│   ├── ActivityLog.cs
│   └── Enums/
│       ├── UserRole.cs
│       ├── TaskPriority.cs
│       └── TaskState.cs
├── DTOs/
│   ├── Auth/ (RegisterDto, LoginDto, AuthResponseDto)
│   └── Tasks/ (CreateTaskDto, UpdateTaskDto, AssignTaskDto, TaskResponseDto, ActivityLogDto)
├── Services/
│   ├── IAuthService.cs / AuthService.cs
│   ├── ITaskService.cs / TaskService.cs
│   └── JwtService.cs
└── Helpers/
    └── JwtSettings.cs
```

## 🔧 Prerequisites
- **.NET 8 SDK** — https://dotnet.microsoft.com/download/dotnet/8.0
- **SQL Server** running as `DESKTOP-E46P7NN\SQLEXPRESS` with SQL Authentication enabled (user `sa`) — the connection string is already configured for this in `appsettings.json`.
- SSMS or Azure Data Studio (only needed if you want to run `schema.sql` manually)

> ⚠️ `sa` / `sa` is fine for local development, but before deploying anywhere shared or public, switch to a strong password and a dedicated login.

## ▶️ How to Run

### 1. Restore packages
```bash
cd SmartTaskEngine
dotnet restore
```

### 2. Create the database — pick one option

**Option A — EF Core migrations (recommended, code-first):**
```bash
dotnet tool install --global dotnet-ef   # only needed once, if not already installed
dotnet ef migrations add InitialCreate
dotnet ef database update
```
This automatically creates the `SmartTaskEngineDb` database and all three tables (`Users`, `TodoTasks`, `ActivityLogs`), matching exactly what's defined in the `Models/` folder.

**Option B — manual SQL script:**
Open `database/schema.sql` in SSMS and run it directly. This produces the exact same table structure without using migrations.

### 3. Run the API
```bash
dotnet run
```
Open the Swagger UI at the URL shown in the console (usually `https://localhost:5001` or `http://localhost:5000`):
```
https://localhost:5001/swagger
```

## 🧪 Testing via Swagger / Postman

1. **Register:** `POST /api/auth/register`
   ```json
   { "username": "rafi", "email": "rafi@example.com", "password": "Passw0rd!" }
   ```
2. **Login:** `POST /api/auth/login` → the response includes a `token`.
3. In Swagger, click **Authorize** and paste the token only. Swagger adds the `Bearer` prefix.
4. **Create task:** `POST /api/tasks`
   ```json
   { "title": "Design DB schema", "description": "First draft", "priority": "High", "dueDate": "2026-08-01T00:00:00Z" }
   ```
5. **List tasks (filter/search):** `GET /api/tasks?status=ToDo&priority=High&search=schema`
6. **Assign task:** `PATCH /api/tasks/1/assign` → `{ "assignedToUserId": 2 }`
7. **Update status:** `PUT /api/tasks/1` → `{ "status": "InProgress" }`
8. **View history:** `GET /api/tasks/1/logs` — task creation, status changes, and assignments are all auto-logged here.
9. **Delete:** `DELETE /api/tasks/1`

## Collaboration endpoints

These sit on top of tasks. A task response now also includes `projectName`, `labels`, `commentCount`, checklist progress, and `isBookmarked`.

- **Projects:** `POST /api/projects`, `GET /api/projects`, `POST /api/projects/{id}/members` with `{ "userId": 2 }`
- **Move a task into a project:** `PATCH /api/tasks/{id}/project` with `{ "projectId": 1 }`
- **Comments:** `POST /api/tasks/{id}/comments` with `{ "body": "Looks good" }`
- **Labels:** `POST /api/labels` with `{ "name": "Backend", "color": "#2563eb" }`, then `POST /api/tasks/{id}/labels/{labelId}`
- **Checklist:** `POST /api/tasks/{id}/checklist` with `{ "title": "Write tests" }`, then `PATCH /api/tasks/{id}/checklist/{itemId}` with `{ "isDone": true }`
- **Bookmark:** `POST /api/tasks/{id}/bookmark`, list with `GET /api/bookmarks`
- **Notifications:** `GET /api/notifications`, `PATCH /api/notifications/{id}/read`, `POST /api/notifications/read-all`
- **Dashboard:** `GET /api/dashboard`
- **People you can assign:** `GET /api/users`

New tables are created automatically the first time the API starts. Re-running `database/schema.sql` still drops and recreates the whole database.

## Notes
- Enums are serialized as strings in JSON (e.g. `"ToDo"`, `"High"`), not integers.
- Passwords are never stored in plain text — they're hashed using `Microsoft.AspNetCore.Identity.PasswordHasher`.
- Every task create/update/assign/delete is automatically written to the `ActivityLogs` table — no extra code needed on your end.
