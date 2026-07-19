# Smart Task & Collaboration Engine

ASP.NET Core (.NET 8) Web API — JWT auth, Task CRUD, assignment, এবং automatic activity log।

## 📁 Folder Structure
```
SmartTaskEngine/
├── SmartTaskEngine.sln
├── SmartTaskEngine.csproj
├── Program.cs
├── appsettings.json              ← connection string + JWT key এখানে
├── appsettings.Development.json
├── database/
│   └── schema.sql                ← চাইলে SSMS দিয়ে ম্যানুয়ালি টেবিল বানানোর script
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
- **SQL Server** running as `DESKTOP-E46P7NN\SQLEXPRESS` with SQL Authentication enabled (user `sa`) — connection string is already set up for this in `appsettings.json`.
- SSMS বা Azure Data Studio (schema.sql চালানোর জন্য, optional)

> ⚠️ `sa` / `sa` লোকাল ডেভে ঠিক আছে, কিন্তু production বা কোনো shared মেশিনে deploy করার আগে অবশ্যই একটা শক্তিশালী পাসওয়ার্ড এবং একটা ডেডিকেটেড লগইন ব্যবহার করবে।

## ▶️ Run করার ধাপ

### 1. Restore packages
```bash
cd SmartTaskEngine
dotnet restore
```

### 2. Database তৈরি — দুইটা অপশন, যেকোনো একটা বেছে নাও

**Option A — EF Core migrations (recommended, code-first):**
```bash
dotnet tool install --global dotnet-ef   # একবারই লাগবে, যদি আগে ইন্সটল না থাকে
dotnet ef migrations add InitialCreate
dotnet ef database update
```
এটা স্বয়ংক্রিয়ভাবে `SmartTaskEngineDb` ডাটাবেস এবং তিনটা টেবিল (`Users`, `TodoTasks`, `ActivityLogs`) বানিয়ে দেবে, ঠিক `Models/` ফোল্ডারে যা ডিফাইন করা আছে তার সাথে মিলিয়ে।

**Option B — manual SQL script:**
`database/schema.sql` ফাইলটা SSMS-এ ওপেন করে সরাসরি রান করে দাও। এটা দিয়ে হুবহু একই টেবিল স্ট্রাকচার তৈরি হবে (migrations ছাড়াই)।

### 3. Run the API
```bash
dotnet run
```
কনসোলে যে URL দেখাবে (সাধারণত `https://localhost:5001` বা `http://localhost:5000`) সেটা দিয়ে Swagger UI ওপেন করো:
```
https://localhost:5001/swagger
```

## 🧪 Testing via Swagger / Postman

1. **Register:** `POST /api/auth/register`
   ```json
   { "username": "rafi", "email": "rafi@example.com", "password": "Passw0rd!" }
   ```
2. **Login:** `POST /api/auth/login` → response-এ একটা `token` পাবে।
3. Swagger-এর উপরে ডানদিকে **Authorize** বাটনে ক্লিক করে `Bearer <token>` বসাও (অথবা Postman-এ Authorization header হিসেবে `Bearer <token>`)।
4. **Create task:** `POST /api/tasks`
   ```json
   { "title": "Design DB schema", "description": "First draft", "priority": "High", "dueDate": "2026-08-01T00:00:00Z" }
   ```
5. **List tasks (filter/search):** `GET /api/tasks?status=ToDo&priority=High&search=schema`
6. **Assign task:** `PATCH /api/tasks/1/assign` → `{ "assignedToUserId": 2 }`
7. **Update status:** `PUT /api/tasks/1` → `{ "status": "InProgress" }`
8. **View history:** `GET /api/tasks/1/logs` — task create, status change, assign — সবকিছুর অটো-লগ এখানে দেখা যাবে।
9. **Delete:** `DELETE /api/tasks/1`

## Notes
- Enum গুলো JSON-এ string হিসেবে আসবে/যাবে (যেমন `"ToDo"`, `"High"`), int না।
- Password কখনো plain text এ সেভ হয় না — `Microsoft.AspNetCore.Identity.PasswordHasher` দিয়ে হ্যাশ করে রাখা হয়।
- প্রতিটা টাস্কের create/update/assign/delete স্বয়ংক্রিয়ভাবে `ActivityLogs` টেবিলে লেখা হয় — কোনো আলাদা কোড লেখা লাগে না।
