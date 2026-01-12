# Educational Time Management System

A full-stack web application for managing educational schedules, courses, assignments, and attendance tracking. Built with **ASP.NET Core 8.0** backend and **React 18** frontend.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![React](https://img.shields.io/badge/React-18-61DAFB?style=flat&logo=react)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?style=flat&logo=typescript)
![SQLite](https://img.shields.io/badge/SQLite-3-003B57?style=flat&logo=sqlite)

---

## 📋 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Prerequisites](#-prerequisites)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Application Flow](#-application-flow)
- [Database Schema](#-database-schema)
- [Project Structure](#-project-structure)
- [Test Credentials](#-test-credentials)

---

## ✨ Features

### User Management
- 🔐 JWT-based authentication with 60-minute token expiry
- 👥 Role-based authorization (Admin, Teacher, Student)
- 📝 User registration and profile management
- 🔑 Secure password hashing with BCrypt

### Course Management
- 📚 Create, update, and delete courses
- 👨‍🏫 Assign teachers to courses
- 📊 Track course credits and descriptions

### Class Sessions
- 🕐 Schedule class sessions with time slots
- 📍 Room assignment and management
- 📅 Recurring schedule support

### Assignments & Submissions
- 📝 Create and publish assignments
- 📤 Student submission system
- ✅ Teacher grading functionality
- 📆 Due date tracking

### Attendance Tracking
- ✓ Record student attendance
- 📊 Bulk attendance recording
- 📈 Attendance history and reports

---

## 🏗 Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              CLIENT LAYER                                    │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                        React 18 + TypeScript                          │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  │  │
│  │  │   Auth      │  │  Dashboard  │  │   Courses   │  │ Assignments │  │  │
│  │  │   Pages     │  │    Page     │  │    Page     │  │    Page     │  │  │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘  │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────────┐   │  │
│  │  │  Sessions   │  │   Users     │  │      Shared Components      │   │  │
│  │  │    Page     │  │   (Admin)   │  │   (Layout, AuthContext)     │   │  │
│  │  └─────────────┘  └─────────────┘  └─────────────────────────────┘   │  │
│  │                              │                                        │  │
│  │                    ┌─────────▼─────────┐                             │  │
│  │                    │   API Service     │                             │  │
│  │                    │  (Axios Client)   │                             │  │
│  │                    └─────────┬─────────┘                             │  │
│  └──────────────────────────────┼────────────────────────────────────────┘  │
└─────────────────────────────────┼────────────────────────────────────────────┘
                                  │ HTTP/HTTPS (JWT Bearer Token)
                                  │
┌─────────────────────────────────┼────────────────────────────────────────────┐
│                              API LAYER                                       │
│  ┌──────────────────────────────▼───────────────────────────────────────┐   │
│  │                     ASP.NET Core 8.0 Web API                         │   │
│  │  ┌─────────────────────────────────────────────────────────────────┐ │   │
│  │  │                      Middleware Pipeline                        │ │   │
│  │  │   CORS → Authentication → Authorization → Controllers          │ │   │
│  │  └─────────────────────────────────────────────────────────────────┘ │   │
│  │                                                                      │   │
│  │  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌─────────────┐ │   │
│  │  │    Auth      │ │   Courses    │ │   Sessions   │ │ Assignments │ │   │
│  │  │  Controller  │ │  Controller  │ │  Controller  │ │ Controller  │ │   │
│  │  └──────────────┘ └──────────────┘ └──────────────┘ └─────────────┘ │   │
│  │  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌─────────────┐ │   │
│  │  │    Users     │ │ Enrollments  │ │ Submissions  │ │ Attendance  │ │   │
│  │  │  Controller  │ │  Controller  │ │  Controller  │ │ Controller  │ │   │
│  │  └──────────────┘ └──────────────┘ └──────────────┘ └─────────────┘ │   │
│  │                              │                                       │   │
│  │                    ┌─────────▼─────────┐                            │   │
│  │                    │    JWT Service    │                            │   │
│  │                    └───────────────────┘                            │   │
│  └──────────────────────────────┬───────────────────────────────────────┘   │
└─────────────────────────────────┼────────────────────────────────────────────┘
                                  │
┌─────────────────────────────────┼────────────────────────────────────────────┐
│                            DATA LAYER                                        │
│  ┌──────────────────────────────▼───────────────────────────────────────┐   │
│  │                    Entity Framework Core 8.0                         │   │
│  │                         (AppDbContext)                               │   │
│  └──────────────────────────────┬───────────────────────────────────────┘   │
│                                 │                                            │
│  ┌──────────────────────────────▼───────────────────────────────────────┐   │
│  │                    SQLite / SQL Server                               │   │
│  │  ┌─────────┐ ┌─────────┐ ┌───────────┐ ┌───────────┐ ┌───────────┐  │   │
│  │  │  Users  │ │ Courses │ │ Sessions  │ │Enrollments│ │ Schedules │  │   │
│  │  └─────────┘ └─────────┘ └───────────┘ └───────────┘ └───────────┘  │   │
│  │  ┌───────────┐ ┌───────────┐ ┌───────────┐ ┌──────────┐ ┌────────┐  │   │
│  │  │Assignments│ │Submissions│ │Attendances│ │ TimeLogs │ │Notific.│  │   │
│  │  └───────────┘ └───────────┘ └───────────┘ └──────────┘ └────────┘  │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────────────────┘
```

### Architecture Patterns

- **Backend**: RESTful API with Controller-Service pattern
- **Frontend**: Component-based architecture with Context API for state management
- **Authentication**: Stateless JWT Bearer tokens
- **Database**: Code-First approach with EF Core migrations

---

## 🛠 Tech Stack

### Backend
| Technology | Version | Purpose |
|------------|---------|---------|
| ASP.NET Core | 8.0 | Web API Framework |
| Entity Framework Core | 8.0.11 | ORM |
| SQLite | 3.x | Development Database |
| SQL Server | 2019+ | Production Database (optional) |
| BCrypt.Net-Next | 4.0.3 | Password Hashing |
| JWT Bearer | 8.0.11 | Authentication |
| Swashbuckle | 6.x | API Documentation |

### Frontend
| Technology | Version | Purpose |
|------------|---------|---------|
| React | 18.x | UI Framework |
| TypeScript | 5.x | Type Safety |
| React Router | 6.x | Client-side Routing |
| Axios | 1.x | HTTP Client |
| CSS3 | - | Styling |

---

## 📦 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (includes npm)
- [Git](https://git-scm.com/)

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/lokeysharma/edutime.git
cd edutime
```

### 2. Start the Backend API

```bash
# Navigate to API project
cd EducationalTimeManagement.Api

# Restore dependencies
dotnet restore

# Run the API (starts on http://localhost:5000)
dotnet run
```

The API will:
- Create the SQLite database automatically
- Seed sample data (users, courses, sessions, assignments)
- Enable Swagger UI at http://localhost:5000/swagger

### 3. Start the Frontend

```bash
# Open a new terminal
cd frontend

# Install dependencies
npm install

# Start the development server (starts on http://localhost:3000)
npm start
```

### 4. Access the Application

| Service | URL |
|---------|-----|
| Frontend | http://localhost:3000 |
| Backend API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |

---

## 📖 API Documentation

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/Auth/register` | Register new user | No |
| POST | `/api/Auth/login` | Login and get JWT | No |
| POST | `/api/Auth/token` | OAuth2 token endpoint | No |
| POST | `/api/Auth/change-password` | Change password | Yes |

### User Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/Users` | Get all users | Admin |
| GET | `/api/Users/{id}` | Get user by ID | Yes |
| GET | `/api/Users/me` | Get current user | Yes |
| GET | `/api/Users/teachers` | Get all teachers | Yes |
| GET | `/api/Users/students` | Get all students | Yes |
| PUT | `/api/Users/{id}` | Update user | Admin |
| DELETE | `/api/Users/{id}` | Delete user | Admin |

### Course Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/Courses` | Get all courses | Yes |
| GET | `/api/Courses/{id}` | Get course by ID | Yes |
| GET | `/api/Courses/my-courses` | Get user's courses | Yes |
| POST | `/api/Courses` | Create course | Teacher/Admin |
| PUT | `/api/Courses/{id}` | Update course | Teacher/Admin |
| DELETE | `/api/Courses/{id}` | Delete course | Admin |

### Session Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/ClassSession` | Get all sessions | Yes |
| GET | `/api/ClassSession/{id}` | Get session by ID | Yes |
| GET | `/api/ClassSession/course/{id}` | Get sessions by course | Yes |
| POST | `/api/ClassSession` | Create session | Teacher/Admin |
| PUT | `/api/ClassSession/{id}` | Update session | Teacher/Admin |
| DELETE | `/api/ClassSession/{id}` | Delete session | Admin |

### Assignment Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/Assignments` | Get all assignments | Yes |
| GET | `/api/Assignments/{id}` | Get assignment by ID | Yes |
| GET | `/api/Assignments/my-assignments` | Get user's assignments | Yes |
| POST | `/api/Assignments` | Create assignment | Teacher/Admin |
| PUT | `/api/Assignments/{id}` | Update assignment | Teacher/Admin |
| DELETE | `/api/Assignments/{id}` | Delete assignment | Teacher/Admin |

### Submission Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/Submissions` | Submit assignment | Student |
| GET | `/api/Submissions/my-submissions` | Get my submissions | Student |
| GET | `/api/Submissions/assignment/{id}` | Get submissions for assignment | Teacher |
| PUT | `/api/Submissions/{id}/grade` | Grade submission | Teacher |

### Attendance Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/Attendance` | Record attendance | Teacher |
| POST | `/api/Attendance/bulk` | Bulk record attendance | Teacher |
| GET | `/api/Attendance/my-attendance` | Get my attendance | Student |
| GET | `/api/Attendance/session/{id}` | Get session attendance | Teacher |

---

## 🔄 Application Flow

### Authentication Flow

```
┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│  User    │────▶│  Login   │────▶│  API     │────▶│  JWT     │
│          │     │  Page    │     │  Server  │     │  Token   │
└──────────┘     └──────────┘     └──────────┘     └──────────┘
                                        │                │
                                        ▼                │
                                  ┌──────────┐          │
                                  │ Validate │          │
                                  │ Password │          │
                                  └──────────┘          │
                                        │                │
                                        ▼                │
┌──────────┐     ┌──────────┐     ┌──────────┐          │
│Protected │◀────│  Store   │◀────│  Return  │◀─────────┘
│  Routes  │     │  Token   │     │  Token   │
└──────────┘     └──────────┘     └──────────┘
```

### Student Workflow

```
┌─────────────────────────────────────────────────────────────┐
│                     STUDENT WORKFLOW                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐ │
│   │  Login  │───▶│Dashboard│───▶│ View    │───▶│ View    │ │
│   │         │    │         │    │ Courses │    │Sessions │ │
│   └─────────┘    └─────────┘    └─────────┘    └─────────┘ │
│                                                     │       │
│                                                     ▼       │
│   ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐ │
│   │  View   │◀───│  View   │◀───│ Submit  │◀───│  View   │ │
│   │ Grades  │    │Submissns│    │  Work   │    │Assignmts│ │
│   └─────────┘    └─────────┘    └─────────┘    └─────────┘ │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Teacher Workflow

```
┌─────────────────────────────────────────────────────────────┐
│                     TEACHER WORKFLOW                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐ │
│   │  Login  │───▶│Dashboard│───▶│ Manage  │───▶│ Create  │ │
│   │         │    │         │    │ Courses │    │Sessions │ │
│   └─────────┘    └─────────┘    └─────────┘    └─────────┘ │
│                       │                             │       │
│                       ▼                             ▼       │
│   ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐ │
│   │  Grade  │◀───│  View   │◀───│ Create  │◀───│ Record  │ │
│   │Submissns│    │Submissns│    │Assignmts│    │Attendnce│ │
│   └─────────┘    └─────────┘    └─────────┘    └─────────┘ │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Admin Workflow

```
┌─────────────────────────────────────────────────────────────┐
│                      ADMIN WORKFLOW                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐ │
│   │  Login  │───▶│Dashboard│───▶│ Manage  │───▶│ Manage  │ │
│   │         │    │(Stats)  │    │  Users  │    │ Courses │ │
│   └─────────┘    └─────────┘    └─────────┘    └─────────┘ │
│                                      │              │       │
│                                      ▼              ▼       │
│                                 ┌─────────┐    ┌─────────┐ │
│                                 │ Create/ │    │ Assign  │ │
│                                 │ Delete  │    │Teachers │ │
│                                 │  Users  │    │         │ │
│                                 └─────────┘    └─────────┘ │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 🗄 Database Schema

### Entity Relationship Diagram

```
┌───────────────────┐       ┌───────────────────┐
│      USERS        │       │     COURSES       │
├───────────────────┤       ├───────────────────┤
│ PK  UserId        │       │ PK  CourseId      │
│     Email         │       │     CourseCode    │
│     PasswordHash  │       │     CourseName    │
│     FirstName     │       │     Description   │
│     LastName      │       │     Credits       │
│     Role          │◀──┐   │ FK  TeacherId     │──┐
│     CreatedAt     │   │   │     CreatedAt     │  │
└───────────────────┘   │   └───────────────────┘  │
         │              │            │             │
         │              └────────────┼─────────────┘
         │                           │
         │    ┌──────────────────────┴─────────────────────┐
         │    │                                            │
         ▼    ▼                                            ▼
┌───────────────────┐       ┌───────────────────┐  ┌───────────────────┐
│   ENROLLMENTS     │       │  CLASS_SESSIONS   │  │   ASSIGNMENTS     │
├───────────────────┤       ├───────────────────┤  ├───────────────────┤
│ PK  EnrollmentId  │       │ PK  Id            │  │ PK  AssignmentId  │
│ FK  StudentId     │──┐    │ FK  CourseId      │  │ FK  CourseId      │
│ FK  CourseId      │  │    │     SessionName   │  │     Title         │
│     EnrollmentDate│  │    │     Description   │  │     Description   │
│     Status        │  │    │     CreatedAt     │  │     DueDate       │
└───────────────────┘  │    └───────────────────┘  │     MaxScore      │
                       │             │             │     IsPublished   │
                       │             │             └───────────────────┘
                       │             │                      │
                       │             ▼                      │
                       │    ┌───────────────────┐          │
                       │    │    SCHEDULES      │          │
                       │    ├───────────────────┤          │
                       │    │ PK  ScheduleId    │          │
                       │    │ FK  ClassSessionId│          │
                       │    │     DayOfWeek     │          │
                       │    │     StartTime     │          │
                       │    │     EndTime       │          │
                       │    │     Room          │          │
                       │    │     EffectiveFrom │          │
                       │    │     EffectiveTo   │          │
                       │    └───────────────────┘          │
                       │             │                      │
                       │             ▼                      ▼
                       │    ┌───────────────────┐  ┌───────────────────┐
                       │    │   ATTENDANCES     │  │   SUBMISSIONS     │
                       │    ├───────────────────┤  ├───────────────────┤
                       │    │ PK  AttendanceId  │  │ PK  SubmissionId  │
                       └───▶│ FK  StudentId     │  │ FK  AssignmentId  │
                            │ FK  ClassSessionId│  │ FK  StudentId     │◀─┘
                            │     Date          │  │     Content       │
                            │     Status        │  │     SubmittedAt   │
                            │     Notes         │  │     Score         │
                            └───────────────────┘  │     Feedback      │
                                                   │     GradedAt      │
                                                   └───────────────────┘

Additional Tables:
┌───────────────────┐       ┌───────────────────┐
│    TIME_LOGS      │       │  NOTIFICATIONS    │
├───────────────────┤       ├───────────────────┤
│ PK  TimeLogId     │       │ PK  NotificationId│
│ FK  UserId        │       │ FK  UserId        │
│     ActivityType  │       │     Title         │
│     StartTime     │       │     Message       │
│     EndTime       │       │     IsRead        │
│     Description   │       │     CreatedAt     │
└───────────────────┘       └───────────────────┘
```

### Table Cardinality

| Relationship | Type | Description |
|--------------|------|-------------|
| User → Course | 1:N | One teacher can teach many courses |
| Course → ClassSession | 1:N | One course has many sessions |
| ClassSession → Schedule | 1:N | One session can have multiple schedules |
| User → Enrollment | 1:N | One student can have many enrollments |
| Course → Enrollment | 1:N | One course can have many enrollments |
| Course → Assignment | 1:N | One course has many assignments |
| Assignment → Submission | 1:N | One assignment has many submissions |
| User → Submission | 1:N | One student has many submissions |
| User → Attendance | 1:N | One student has many attendance records |
| ClassSession → Attendance | 1:N | One session has many attendance records |

---

## 📁 Project Structure

```
edutime/
├── EducationalTimeManagement.Api/     # Backend API
│   ├── Controllers/                   # API Controllers
│   │   ├── AuthController.cs          # Authentication endpoints
│   │   ├── UsersController.cs         # User management
│   │   ├── CoursesController.cs       # Course management
│   │   ├── ClassSessionController.cs  # Session management
│   │   ├── AssignmentsController.cs   # Assignment management
│   │   ├── SubmissionsController.cs   # Submission handling
│   │   ├── AttendanceController.cs    # Attendance tracking
│   │   └── EnrollmentsController.cs   # Enrollment management
│   ├── Data/
│   │   └── AppDbContext.cs            # EF Core DbContext
│   ├── Models/                        # Entity models
│   │   ├── User.cs
│   │   ├── Course.cs
│   │   ├── ClassSession.cs
│   │   ├── Assignment.cs
│   │   ├── Submission.cs
│   │   ├── Attendance.cs
│   │   ├── Enrollment.cs
│   │   ├── Schedule.cs
│   │   ├── TimeLog.cs
│   │   ├── Notification.cs
│   │   └── DTOs/                      # Data Transfer Objects
│   ├── Services/
│   │   └── JwtService.cs              # JWT token generation
│   ├── Program.cs                     # Application entry point
│   ├── appsettings.json               # Configuration
│   └── app.db                         # SQLite database (auto-generated)
│
├── frontend/                          # React Frontend
│   ├── public/
│   │   └── index.html
│   ├── src/
│   │   ├── components/
│   │   │   └── Layout/                # Sidebar layout
│   │   ├── context/
│   │   │   └── AuthContext.tsx        # Authentication state
│   │   ├── pages/
│   │   │   ├── Auth/                  # Login & Register
│   │   │   ├── Dashboard/             # Dashboard
│   │   │   ├── Courses/               # Course management
│   │   │   ├── Sessions/              # Session management
│   │   │   ├── Assignments/           # Assignment management
│   │   │   └── Users/                 # User management (Admin)
│   │   ├── services/
│   │   │   └── api.ts                 # Axios API client
│   │   ├── types/
│   │   │   └── index.ts               # TypeScript interfaces
│   │   ├── App.tsx                    # Main app with routing
│   │   └── index.tsx                  # Entry point
│   └── package.json
│
├── .gitignore
├── edumgm.sln                         # Visual Studio solution
└── README.md
```

---

## 🔑 Test Credentials

The application seeds these test accounts on first run:

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@example.com | Admin123! |
| Teacher | teacher@example.com | Teacher123! |
| Teacher | teacher2@example.com | Teacher123! |
| Student | student@example.com | Student123! |
| Student | student2@example.com | Student123! |
| Student | student3@example.com | Student123! |

---

## 🔧 Configuration

### Backend Configuration (appsettings.json)

```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=app.db",
    "DefaultConnection": "Server=...;Database=...;"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-chars",
    "Issuer": "EducationalTimeManagement",
    "Audience": "EducationalTimeManagementUsers",
    "ExpirationMinutes": 60
  }
}
```

### Frontend Configuration

The API base URL is configured in `frontend/src/services/api.ts`:

```typescript
const API_BASE_URL = 'http://localhost:5000/api';
```

---

## 📄 License

This project is licensed under the MIT License.

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request
