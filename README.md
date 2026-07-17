<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Blazor-WASM-512BD4?style=for-the-badge&logo=blazor&logoColor=white" alt="Blazor WASM" />
  <img src="https://img.shields.io/badge/AWS-Rekognition-FF9900?style=for-the-badge&logo=amazonaws&logoColor=white" alt="AWS Rekognition" />
  <img src="https://img.shields.io/badge/PostgreSQL-Supabase-3ECF8E?style=for-the-badge&logo=supabase&logoColor=white" alt="Supabase" />
  <br />
  <img src="https://img.shields.io/badge/Redis-Cache-DC382D?style=for-the-badge&logo=redis&logoColor=white" alt="Redis" />
  <img src="https://img.shields.io/badge/TailwindCSS-v3-06B6D4?style=for-the-badge&logo=tailwindcss&logoColor=white" alt="TailwindCSS" />
 <img src="https://img.shields.io/badge/Azure-Deployment-0089D6?style=for-the-badge&logo=microsoftazure&logoColor=white" alt="Azure Deployment" />
</p>

<h1 align="center">🎯 Attencial</h1>

<p align="center">
  <strong>A production-grade facial recognition attendance management system for academic institutions.</strong>
</p>

<p align="center">
  <a href="https://attencial.live">🌐 (deployment went down due to azure bills) Live Demo — attencial.live</a>
</p>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Key Features](#-key-features)
- [System Architecture](#-system-architecture)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Database Schema](#-database-schema)
- [API Reference](#-api-reference)
- [The 8-Layer Attendance Pipeline](#-the-8-layer-attendance-pipeline)
- [Face Recognition Flow](#-face-recognition-flow)
- [Resilience & Fault Tolerance](#-resilience--fault-tolerance)
- [Real-Time Dashboard](#-real-time-dashboard)
- [Security Architecture](#-security-architecture)
- [Frontend Pages](#-frontend-pages)
- [Getting Started](#-getting-started)
- [Environment Variables](#-environment-variables)
- [Deployment](#-deployment)
- [Engineering Decisions](#-engineering-decisions)

---

## 🔭 Overview

**Attencial** is an end-to-end biometric attendance management platform designed to eliminate manual roll calls and proxy attendance fraud in universities. It leverages **AWS Rekognition** for facial detection and identification, **Supabase Realtime WebSockets** for live dashboards, and an **8-layered verification pipeline** to ensure every attendance record is authentic, auditable, and tamper-proof.

The system serves three distinct user roles — **Students**, **Professors**, and **Administrators** — each with dedicated dashboards, workflows, and access-controlled API endpoints.

### What Makes This Different

| Traditional Systems | Attencial |
|---|---|
| Manual roll call or RFID cards | Biometric face scan — no tokens to share |
| Easy proxy attendance | AWS Rekognition face matching with ≥70% similarity threshold |
| No abuse detection | 8-layer pipeline with IP rate limiting, enrollment checks, and abuse logging |
| Delayed reports | Supabase Realtime WebSocket — live updates within 1 second |
| No resilience | Polly circuit breaker + exponential retry on all cloud calls |

---

## ✨ Key Features

### 🧑‍🎓 For Students
- **Face Enrollment** — Capture 3 photos via webcam, indexed into AWS Rekognition collection
- **One-Scan Attendance** — Scan QR code → face verification → attendance marked in seconds
- **Personal Dashboard** — Per-course attendance percentages with color-coded warnings 
- **Course Enrollment** — Browse available courses, enroll, and track enrollment status
- **Attendance Appeals** — Submit appeals for missed attendance with professor review workflow
- **Profile Management** — View enrollment status, face registration details, and account info

### 👨‍🏫 For Professors
- **Session Management** — Create timed attendance sessions (5–60 min) with auto-generated QR codes and shareable links
- **Live Dashboard** — Watch students check in via Supabase Realtime WebSockets with polling fallback
- **Course Analytics** — Per-session attendance counts, student-level breakdowns, and abuse log monitoring
- **CSV Export** — Download attendance reports for any course with one click
- **Enrollment Review** — Approve or reject student enrollment requests
- **Appeal Review** — Review and approve/reject student attendance appeals
- **Faculty HR** — Face-verified check-in/check-out with automatic shift hour calculation

### 🔐 For Administrators
- **Leave Management** — Review and approve/reject faculty leave requests
- **Short Shift Oversight** — Flag and approve faculty members with <8 hour shifts
- **Abuse Monitoring** — System-wide abuse logs for unauthorized attendance attempts

---

## 🏗 System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        FRONTEND (Blazor WASM)                   │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌────────────────────┐  │
│  │  Login/  │ │  Face    │ │ Session  │ │    Dashboards      │  │
│  │ Register │ │ Enroll   │ │ Manager  │ │ Student/Prof/Admin │  │
│  └────┬─────┘ └────┬─────┘ └────┬─────┘ └────────┬───────────┘  │
│       │            │            │                │              │
│  ┌────┴────────────┴────────────┴────────────────┴────────────┐ │
│  │  JS Interop: auth.js | camera.js | realtime.js | parallax  │ │
│  └────────────────────────────────────────────────────────────┘ │
└───────────────────────────┬─────────────────────────────────────┘
                            │ HTTPS / JWT Bearer
┌───────────────────────────┴─────────────────────────────────────┐
│                     BACKEND (ASP.NET Core 10 API)               │
│                                                                 │
│  ┌─────────────┐  ┌──────────────────┐  ┌────────────────────┐  │
│  │ Controllers │  │   Middleware     │  │   Validators       │  │
│  │ (11 files)  │  │ GlobalException  │  │  FluentValidation  │  │
│  └──────┬──────┘  └──────────────────┘  └────────────────────┘  │
│         │                                                       │
│  ┌──────┴──────┐  ┌──────────────────┐  ┌───────────────────┐   │
│  │  Services   │  │  Repositories    │  │  EF Core          │   │
│  │ Face/Attend │  │ Student/Course   │  │  AppDbContext     │   │
│  └──────┬──────┘  └──────────────────┘  └──────────┬────────┘   │
│         │                                          │            │
└─────────┼──────────────────────────────────────────┼────────────┘
          │                                          │
    ┌─────┴──────┐                          ┌────────┴────────┐
    │    AWS     │                          │   PostgreSQL    │
    │ Rekognition│                          │   (Supabase)    │
    │ Collection │                          │  14 Tables      │
    └─────┬──────┘                          └────────┬────────┘
          │                                          │
    ┌─────┴──────┐                          ┌────────┴────────┐
    │   Polly    │                          │   Supabase      │
    │ Retry +    │                          │   Realtime      │
    │ Circuit    │                          │   WebSockets    │
    │ Breaker    │                          └─────────────────┘
    └────────────┘
```

### Request Flow

```
Client (Blazor WASM)
  └──▶ HTTP Request + JWT Bearer Token
        └──▶ CORS Middleware
              └──▶ Global Exception Middleware (RFC 7807 ProblemDetails)
                    └──▶ JWT Authentication
                          └──▶ Role-Based Authorization ([Authorize(Roles = "...")])
                                └──▶ FluentValidation (6 validators)
                                      └──▶ Controller → Service → Repository → Database
```

---

## 🛠 Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Runtime** | .NET 10 | Latest .NET with native AOT support |
| **Backend** | ASP.NET Core 10 Web API | RESTful API with controller-based routing |
| **Frontend** | Blazor WebAssembly | C# single-page application running in the browser |
| **UI Framework** | Tailwind CSS v3 + Custom Design System | Material Design 3 inspired color tokens with Playfair Display + Hanken Grotesk typography |
| **Database** | PostgreSQL via Supabase | Cloud-hosted relational database with connection pooling |
| **ORM** | Entity Framework Core 10 | Code-first migrations, LINQ queries, relationship mapping |
| **Authentication** | JWT Bearer + BCrypt.Net | Stateless token-based auth with secure password hashing |
| **Face Recognition** | AWS Rekognition | Face detection, indexing, and similarity search |
| **Caching** | Redis (StackExchange.Redis) | Distributed rate limiting with in-memory fallback |
| **Real-Time** | Supabase Realtime (WebSockets) | Live attendance feed with C# polling fallback |
| **Resilience** | Polly v8 | Exponential retry (3x) + circuit breaker (5 failures) |
| **Validation** | FluentValidation v12 | Declarative request validation on 6 DTOs |
| **Animations** | GSAP + Custom JS | Scroll-triggered animations, parallax effects, particle canvas |
| **QR Codes** | qrcode.js | Client-side QR code generation for attendance links |

### NuGet Packages (API)

```xml
AWSSDK.Rekognition          — Face detection, indexing, and search
BCrypt.Net-Next             — Password hashing (bcrypt)
FluentValidation            — Request DTO validation
Microsoft.AspNetCore.Authentication.JwtBearer — JWT token validation
Microsoft.EntityFrameworkCore.Design/Tools    — EF Core migrations
Npgsql.EntityFrameworkCore.PostgreSQL         — PostgreSQL provider
Microsoft.Extensions.Caching.StackExchangeRedis — Distributed cache
Polly                       — Resilience (retry + circuit breaker)
Microsoft.AspNetCore.Components.WebAssembly.Server — Blazor WASM hosting
```

---

## 📁 Project Structure

The solution follows a **clean 3-project architecture** with clear separation of concerns:

```
Attencial/
├── Attencial.API/                    # Backend — ASP.NET Core 10 Web API
│   ├── Controllers/                  # 11 API controllers (Auth, Attendance, Enrollment, etc.)
│   │   ├── AppealController.cs       # Student attendance appeal CRUD
│   │   ├── AttendanceController.cs   # Session management + 8-layer marking pipeline
│   │   ├── AuthController.cs         # Register, Login, Me endpoints
│   │   ├── CourseEnrollmentController.cs  # Student↔Course enrollment management
│   │   ├── EnrollmentController.cs   # Face enrollment (detect, index, status, reset)
│   │   ├── FacultyController.cs      # Faculty HR check-in/check-out + admin review
│   │   ├── LeaveController.cs        # Leave request submission + admin review
│   │   ├── ProfessorController.cs    # Course analytics, session history, CSV export
│   │   ├── SeedController.cs         # Development data seeding (profile/course creation)
│   │   ├── StudentController.cs      # Student profile + per-course attendance stats
│   │   └── TestControllers.cs        # Debug endpoint (development only)
│   ├── Data/
│   │   └── AppDbContext.cs           # EF Core context — 14 DbSets, unique indexes, relationships
│   ├── Filters/
│   │   └── ValidationFilter.cs       # Endpoint filter for automatic FluentValidation
│   ├── Middleware/
│   │   └── GlobalExceptionMiddleware.cs  # RFC 7807 ProblemDetails error responses
│   ├── Models/                       # 12 Entity models (User, Student, Professor, Course, etc.)
│   ├── Repositories/                 # Repository pattern (IStudentRepository, ICourseRepository)
│   ├── Services/                     # Business logic (FaceService, AttendanceService)
│   ├── Validators/                   # 6 FluentValidation validators
│   ├── Migrations/                   # EF Core database migrations
│   └── Program.cs                    # DI container, middleware pipeline, JWT config
│
├── Attencial.Client/                 # Frontend — Blazor WebAssembly
│   ├── Pages/                        # 12 page components (code-behind .cs pattern)
│   │   ├── Home.cs                   # Landing page with animations and parallax
│   │   ├── Login.cs                  # JWT login form
│   │   ├── Register.cs               # User registration (Student/Professor)
│   │   ├── Dashboard.cs              # Role-based routing dashboard
│   │   ├── EnrollFace.cs             # Webcam face enrollment (3 photos)
│   │   ├── Attend.cs                 # Student attendance marking (?token=)
│   │   ├── Session.cs                # Professor session management + live feed
│   │   ├── StudentDashboard.cs       # Per-course attendance stats
│   │   ├── ProfessorDashboard.cs     # Course analytics + CSV export
│   │   ├── Courses.cs                # Course enrollment management
│   │   ├── Profile.cs                # User profile page
│   │   └── EnrollmentReview.cs       # Professor enrollment review
│   ├── Components/
│   │   └── FaceCaptureComponent.cs   # Reusable webcam capture component
│   ├── Services/
│   │   └── UnauthorizedHttpHandler.cs  # Auto-redirect on 401 responses
│   ├── Layout/                       # MainLayout + NavMenu
│   ├── wwwroot/
│   │   ├── css/
│   │   │   ├── tailwind-build.css    # Compiled Tailwind CSS
│   │   │   └── app.css               # Custom animations, loading screen, utilities
│   │   ├── js/
│   │   │   ├── auth.js               # localStorage token management + JWT parsing
│   │   │   ├── camera.js             # MediaDevices API webcam capture
│   │   │   ├── realtime.js           # Supabase Realtime WebSocket client
│   │   │   ├── animations.js         # GSAP scroll animations + particle canvas
│   │   │   └── parallax.js           # Geometric spin + parallax effects
│   │   └── index.html                # SPA entry point
│   ├── tailwind.config.js            # Material Design 3 color tokens + typography
│   └── Program.cs                    # WASM host builder + HttpClient config
│
├── Attencial.Shared/                 # Shared class library (referenced by API + Client)
│   ├── Dtos/                         # 11 Data Transfer Objects
│   │   ├── ApiResponse.cs            # Generic API response wrapper ApiResponse<T>
│   │   ├── LoginRequest.cs           # Email + Password
│   │   ├── RegisterRequest.cs        # Email, Password, Role, FullName, RollNumber
│   │   ├── CreateSessionRequest.cs   # CourseId + ExpiryMinutes
│   │   ├── SessionResponseDto.cs     # Session details + attendance URL
│   │   ├── AttendanceMarkRequest.cs  # Token + Base64 Image + DeviceId
│   │   ├── AttendanceMarkResponse.cs # Student name, roll, course, timestamp
│   │   ├── AttendanceTokenValidateResponse.cs  # Session info for token validation
│   │   ├── CourseEnrollmentRequest.cs # CourseId
│   │   ├── LeaveRequestCreateRequest.cs  # LeaveType, dates, reason
│   │   └── LeaveRequestReviewRequest.cs  # Status + ReviewNote
│   └── Enums/
│       └── EnrollmentStatus.cs       # Pending, Trained, Failed
│
├── ROADMAP.md                        # 25-day development roadmap
└── Attencial.slnx                    # Solution file
```

---

## 🗄 Database Schema

The system uses **12 tables** in PostgreSQL (Supabase), managed through EF Core Code-First migrations:

```
┌────────────────────────────────────────────────────────┐
│                        Users                           │
│ (Id, Email, PasswordHash, Role, CreatedAt)             │
└───────────┬────────────────────────────┬───────────────┘
            │ 1                          │ 1
            ▼ 0..1                       ▼ 0..1
┌───────────────────────────┐  ┌─────────────────────────┐
│         Students          │  │       Professors        │
│ (Id, UserId, FullName,    │  │ (Id, UserId, FullName,  │
│  RollNumber,              │  │  Department, CreatedAt) │
│  RekognitionExternalId,   │  └──────────┬──────────────┘
│  EnrollmentStatus,        │             │ 1
│  CreatedAt)               │             │
└─────┬───┬───┬───┬─────────┘             ▼ 0..*
      │   │   │   │   │                ┌─────────────────────┐
      │   │   │   │   └──────────────▶│       Courses       │
      │   │   │   │   1                │ (Id, CourseCode,    │
      │   │   │   │                    │  Name, ProfessorId, │
      │   │   │   │                    │  CreatedAt)         │
      │   │   │   │                    └──────────┬──────────┘
      │   │   │   │                               │ 1
      │   │   │   └─────────────────┐             ▼ 0..*
      │   │   │ 1                   │ 1         ┌────────────────────────┐
      │   │   ▼ 0..*                ▼ 0..*      │   AttendanceSessions   │
      │   │ ┌───────────────┐ ┌───────────────┐ │ (Id, CourseId,         │
      │   │ │  Enrollments  │ │  Enrollment   │ │  ProfessorId,          │
      │   │ │ (Id, Student  │ │   Requests    │ │  StartTime, EndTime,   │
      │   │ │  Id, CourseId,│ │ (Id, Student  │ │  IsActive, CreatedAt)  │
      │   │ │  EnrolledAt)  │ │  Id, CourseId,│ └──────┬─────┬─────┬─────┘
      │   │ └───────▲───────┘ │  Status, Note,│        │ 1   │ 1   │ 1
      │   │         │         │  RequestedAt, │        │     │     │
      │   │         │         │  ReviewedAt)  │        │     │     └──────────────┐
      │   │         │         └───────▲───────┘        │     │                    │
      │   │         └────────┐        │                ▼     ▼                    ▼
      │   │                  │ 1      │ 1          ┌───────┐ ┌──────────────┐ ┌──────────────┐
      │   │                  └────────┴────────────┤Online │ │ Attendance   │ │  AbuseLogs   │
      │   │ 1                                      │Tokens │ │   Records    │ │ (Id, Session │
      │   ▼ 0..*                                   │(Id,   │ │ (Id, Session │ │  Id, Student │
      │ ┌──────────────────────────┐               │Token, │ │  Id, Student │ │  Id, Abuse   │
      │ │       FaceVectors        │               │Session│ │  Id,         │ │  Type,       │
      │ │ (Id, StudentId,          │               │Id,    │ │  Confidence, │ │  Details,    │
      │ │  ProfessorId,            │               │Expiry,│ │  DeviceId,   │ │  DeviceId,   │
      │ │  RekognitionExternalId,  │               │Expires│ │  MarkedAt)   │ │  IpAddress,  │
      │ │  RekognitionFaceId,      │               │At,    │ └──────▲───────┘ │  CreatedAt)  │
      │ │  ImageUrl, CreatedAt)    │               │Active)│        │         └──────▲───────┘
      │ └───────▲──────────────────┘               └───────┘        │                │
      │         │                                                   │                │
      └─────────┼───────────────────────────────────────────────────┘                │
                │ 0..1 (Professors can also register face vectors)                   │
                └────────────────────────────────────────────────────────────────────┘
```

### Key Constraints & Indexes

| Constraint | Type | Purpose |
|-----------|------|---------|
| `AttendanceRecord(SessionId, StudentId)` | Unique Index | Prevents duplicate attendance per session |
| `Enrollment(StudentId, CourseId)` | Unique Index | One enrollment per student per course |
| `EnrollmentRequest(StudentId, CourseId)` | Unique Index | One pending request per student per course |
| `AttendanceAppeal(SessionId, StudentId)` | Unique Index | One appeal per student per session |
| `OnlineAttendanceToken(Token)` | Unique Index | Prevents token collisions |
| `Student(UserId)` | Unique Index | One student profile per user account |
| `Professor(UserId)` | Unique Index | One professor profile per user account |
| `User(Email)` | Unique Index | Prevents duplicate email registration |

---

## 📡 API Reference

### Authentication (`/api/auth`)

| Method | Endpoint | Auth | Description |
|--------|---------|------|-------------|
| `POST` | `/api/auth/register` | Public | Register a new Student/Professor account |
| `POST` | `/api/auth/login` | Public | Login → returns JWT token |
| `GET` | `/api/auth/me` | Bearer | Get current user profile from JWT claims |

### Face Enrollment (`/api/enrollment`)

| Method | Endpoint | Auth | Description |
|--------|---------|------|-------------|
| `POST` | `/api/enrollment/detect` | Student | Detect face in Base64 image (pre-check) |
| `POST` | `/api/enrollment/enroll` | Student | Full enrollment: 3 photos → AWS IndexFaces |
| `GET` | `/api/enrollment/status` | Student | Get face enrollment status (Pending/Trained/Failed) |
| `DELETE` | `/api/enrollment/reset` | Student | Delete all face vectors and re-enroll |

### Attendance Sessions (`/api/attendance`)

| Method | Endpoint | Auth | Description |
|--------|---------|------|-------------|
| `POST` | `/api/attendance/sessions` | Professor | Create new session with crypto-random token |
| `GET` | `/api/attendance/sessions/active` | Professor | Get currently active session |
| `GET` | `/api/attendance/sessions/{id}` | Professor | Get session details (with auto-expiry check) |
| `DELETE` | `/api/attendance/sessions/{id}/end` | Professor | Manually end a session |
| `GET` | `/api/attendance/professor/courses` | Professor | List professor's courses (for dropdown) |
| `GET` | `/api/attendance/sessions/validate` | Public | Validate attendance token (Layer 1 & 2) |
| `POST` | `/api/attendance/mark` | Public | Mark attendance (full 8-layer pipeline) |
| `GET` | `/api/attendance/courses/{id}/enrolled-students` | Professor | List enrolled students |
| `GET` | `/api/attendance/sessions/{id}/records` | Professor | Get attendance records for session |
| `GET` | `/api/attendance/config/supabase-realtime` | Professor | Get Supabase config for WebSocket |

### Course Enrollment (`/api/course-enrollment`)

| Method | Endpoint | Auth | Description |
|--------|---------|------|-------------|
| `POST` | `/api/course-enrollment/enroll` | Student | Request enrollment in a course |
| `GET` | `/api/course-enrollment/my-courses` | Student | List enrolled courses |
| `DELETE` | `/api/course-enrollment/drop/{courseId}` | Student | Drop a course |
| `GET` | `/api/course-enrollment/available` | Student | Browse available courses |

### Student (`/api/students`)

| Method | Endpoint | Auth | Description |
|--------|---------|------|-------------|
| `GET` | `/api/students/me/attendance` | Student | Per-course attendance stats with percentages |
| `GET` | `/api/students/me/profile` | Student | Student profile details |

### Professor Analytics (`/api/professor`)

| Method | Endpoint | Auth | Description |
|--------|---------|------|-------------|
| `GET` | `/api/professor/courses` | Professor | List all courses |
| `GET` | `/api/professor/courses/{id}/sessions` | Professor | Session history with present/absent counts |
| `GET` | `/api/professor/courses/{id}/abuselogs` | Professor | Abuse logs for a course |
| `GET` | `/api/professor/courses/{id}/export` | Professor | CSV attendance report download |
| `GET` | `/api/professor/courses/{id}/students` | Professor | Students with attendance breakdown |

### Faculty HR (`/api/faculty`)

| Method | Endpoint | Auth | Description |
|--------|---------|------|-------------|
| `POST` | `/api/faculty/attendance/checkin` | Professor | Face-verified check-in |
| `POST` | `/api/faculty/attendance/checkout` | Professor | Face-verified check-out + hours calc |
| `GET` | `/api/faculty/attendance/history` | Professor | Faculty attendance history |
| `GET` | `/api/faculty/attendance/today` | Professor | Today's check-in status |
| `GET` | `/api/admin/faculty/pending` | Admin | Pending short-shift reviews |
| `PUT` | `/api/admin/faculty/{id}/approve` | Admin | Approve short shift |

### Leave Management

| Method | Endpoint | Auth | Description |
|--------|---------|------|-------------|
| `POST` | `/api/faculty/leave` | Professor | Submit leave request |
| `GET` | `/api/faculty/leave` | Professor | View own leave requests |
| `GET` | `/api/admin/leave/pending` | Admin | View pending leave requests |
| `PUT` | `/api/admin/leave/{id}/review` | Admin | Approve/reject leave request |

### Appeals (`/api/appeals`)

| Method | Endpoint | Auth | Description |
|--------|---------|------|-------------|
| `POST` | `/api/appeals` | Student | Submit attendance appeal |
| `GET` | `/api/appeals/my` | Student | View own appeals |
| `GET` | `/api/appeals/professor` | Professor | View appeals for your courses |
| `PUT` | `/api/appeals/{id}/review` | Professor | Approve/reject appeal |

---

## 🛡 The 8-Layer Attendance Pipeline

The core of Attencial is an **8-layer verification pipeline** that processes every attendance marking request. Each layer must pass before the next executes, creating a defense-in-depth architecture:

```
 ┌─── Layer 1: Token Validation ───────────────────────────────────┐
 │  Verify the attendance token exists, is active, and not expired │
 └──────────────────────┬──────────────────────────────────────────┘
                        │ ✓ Pass
 ┌──────────────────────┴──────────────────────────────────────────┐
 │  Layer 2: Rate Limiting (Redis)                                 │
 │  Max 2 marking attempts per device per token per minute         │
 │  Uses IDistributedCache with Redis or in-memory fallback        │
 └──────────────────────┬──────────────────────────────────────────┘
                        │ ✓ Pass
 ┌──────────────────────┴──────────────────────────────────────────┐
 │  Layer 3: Face Detection (AWS Rekognition DetectFaces)          │
 │  Verifies that the submitted image contains a human face        │
 └──────────────────────┬──────────────────────────────────────────┘
                        │ ✓ Pass
 ┌──────────────────────┴──────────────────────────────────────────┐
 │  Layer 4: Face Identification (AWS Rekognition SearchFacesByImage)
 │  Matches the face against the enrolled collection               │
 │  Requires ≥70% similarity score                                 │
 └──────────────────────┬──────────────────────────────────────────┘
                        │ ✓ Pass
 ┌──────────────────────┴──────────────────────────────────────────┐
 │  Layer 5: Student Lookup                                        │
 │  RekognitionFaceId → FaceVectors → Student profile              │
 │  Verifies EnrollmentStatus == "Trained"                         │
 └──────────────────────┬──────────────────────────────────────────┘
                        │ ✓ Pass
 ┌──────────────────────┴──────────────────────────────────────────┐
 │  Layer 6: Course Enrollment Check                               │
 │  Is this student enrolled in the course for this session?       │
 │  ✗ Fail → Creates AbuseLog entry with details + IP              │
 └──────────────────────┬──────────────────────────────────────────┘
                        │ ✓ Pass
 ┌──────────────────────┴──────────────────────────────────────────┐
 │  Layer 7: Duplicate Check                                       │
 │  Has this student already marked attendance for this session?   │
 │  Uses unique index constraint on (SessionId + StudentId)        │
 └──────────────────────┬──────────────────────────────────────────┘
                        │ ✓ Pass
 ┌──────────────────────┴──────────────────────────────────────────┐
 │  Layer 8: Record Insertion                                      │
 │  Insert AttendanceRecord with confidence score + device ID      │
 │  ✓ Return success with student name, roll, course, timestamp    │
 └─────────────────────────────────────────────────────────────────┘
```

---

## 🧬 Face Recognition Flow

### Enrollment (One-Time Setup)

```
Student opens /enroll
    │
    ▼
┌─ Capture 3 Photos via Webcam ────────────────────────────┐
│  camera.js → getUserMedia → canvas.toDataURL → Base64    │
└──────┬───────────────────────────────────────────────────┘
       │
       ▼
┌─ POST /api/enrollment/enroll ─────────────────────────────┐
│  For each photo:                                          │
│    1. FaceService.DetectFaceAsync(base64)                 │
│       → AWS DetectFaces → verify face exists              │
│    2. FaceService.IndexFaceAsync(base64, rollNumber)      │
│       → AWS IndexFaces → returns FaceId (GUID)            │
│    3. Save FaceVector { RekognitionFaceId, ExternalId }   │
│                                                           │
│  After 3 successful indexes:                              │
│    Student.EnrollmentStatus = "Trained"                   │
└───────────────────────────────────────────────────────────┘
```

### Attendance Verification (Every Session)

```
Student scans QR code → /attend?token=abc123
    │
    ▼
Webcam capture → Base64 image
    │
    ▼
POST /api/attendance/mark { token, image, deviceId }
    │
    ▼
FaceService.SearchFaceAsync(base64)
    │
    ├── AWS SearchFacesByImage (threshold: 70%)
    │   └── Returns: { faceId: "guid", similarity: 94.7 }
    │
    ▼
FaceVector lookup → Student profile → Course enrollment check → Record inserted
```

---

## 🔄 Resilience & Fault Tolerance

All AWS Rekognition calls are wrapped in a **Polly v8 resilience pipeline** with two strategies:

### Exponential Retry (3 attempts)

```
Attempt 1: Immediate
Attempt 2: ~1s delay (with jitter)
Attempt 3: ~2s delay (with jitter)
Attempt 4: ~4s delay (with jitter)
```

### Circuit Breaker

```
CLOSED state (normal) ──▶ 5 consecutive failures in 30s ──▶ OPEN state (all calls rejected)
                                                                       │
                                              30s cooldown ◀──────────┘
                                                   │
                                            HALF-OPEN state
                                            (allow 1 test call)
                                                   │
                                          ┌────────┴────────┐
                                     Success → CLOSED    Failure → OPEN
```

**Smart exception filtering:** The pipeline only retries transient errors. `InvalidParameterException` (bad image) and `ResourceAlreadyExistsException` (collection exists) are not retried — they fail immediately to avoid wasting time on unrecoverable errors.

---

## 📡 Real-Time Dashboard

The professor's session management page receives live attendance updates through a **dual-channel architecture**:

### Primary: Supabase Realtime WebSockets

```javascript
// realtime.js subscribes to PostgreSQL changes
client.channel('public-AttendanceRecords')
  .on('postgres_changes', {
    event: 'INSERT',
    schema: 'public',
    table: 'AttendanceRecords',
    filter: 'SessionId=eq.' + sessionId
  }, (payload) => {
    dotNetHelper.invokeMethodAsync('OnAttendanceMarkedRealtime',
      payload.new.StudentId,
      payload.new.Confidence,
      payload.new.MarkedAt
    );
  })
  .subscribe();
```

### Fallback: C# Polling

If WebSocket configuration is missing or connection fails, the system gracefully falls back to periodic HTTP polling:

```javascript
if (!anonKey || anonKey.includes("placeholder") || anonKey.length < 20) {
    return "polling";  // Signals Blazor to use polling fallback
}
```

---

## 🔐 Security Architecture

| Layer | Implementation |
|-------|---------------|
| **Authentication** | JWT Bearer tokens with configurable expiry (default: 60 min) |
| **Password Storage** | BCrypt hashing (BCrypt.Net-Next) — no plaintext storage |
| **Authorization** | Role-based `[Authorize(Roles = "Student/Professor/Admin")]` on every endpoint |
| **Token Generation** | 48 bytes → 64-char Base64 URL-safe tokens via `RandomNumberGenerator` |
| **Rate Limiting** | Redis-backed counters with TTL (2 attempts/device/token/minute) |
| **Abuse Logging** | Unauthorized enrollment attempts logged with IP, device ID, and timestamp |
| **CORS** | Locked to specific allowed origins only |
| **Error Masking** | Stack traces hidden in production (RFC 7807 ProblemDetails format) |
| **HTTPS/HSTS** | HSTS headers + HTTPS redirect in production |
| **Auto-Logout** | `UnauthorizedHttpHandler` — automatic token cleanup + redirect on 401 |
| **Input Validation** | FluentValidation on 6 request DTOs (email format, password length, date ranges) |

### Validated Request DTOs

| Validator | Rules |
|-----------|-------|
| `RegisterRequestValidator` | Email format, password ≥6 chars, role must be Student/Professor/Admin, roll number required for students |
| `LoginRequestValidator` | Email + password required |
| `CreateSessionRequestValidator` | CourseId > 0, expiry between 5–60 minutes |
| `AttendanceMarkRequestValidator` | Token, image, and device ID required |
| `LeaveRequestCreateRequestValidator` | Valid leave type, future dates, reason 10–1000 chars |
| `LeaveRequestReviewRequestValidator` | Status Approved/Rejected, review note 10–500 chars |

---

## 🖥 Frontend Pages

| Page | Route | Role | Description |
|------|-------|------|-------------|
| **Home** | `/` | Public | Landing page with animated hero, feature cards, parallax effects |
| **Login** | `/login` | Public | JWT login form with validation feedback |
| **Register** | `/register` | Public | Role-based registration (Student/Professor) |
| **Dashboard** | `/dashboard` | All | Role-aware routing hub |
| **Face Enrollment** | `/enroll` | Student | 3-photo webcam capture → AWS Rekognition indexing |
| **Attend** | `/attend?token=` | Student | QR-linked attendance marking with face scan |
| **Session Manager** | `/session` | Professor | Create sessions, view QR code, live attendance feed |
| **Student Dashboard** | `/student-dashboard` | Student | Per-course attendance percentages with color coding |
| **Professor Dashboard** | `/professor-dashboard` | Professor | Course analytics, session history, CSV export |
| **Courses** | `/courses` | Student | Browse and enroll in available courses |
| **Profile** | `/profile` | All | User profile and enrollment status |
| **Enrollment Review** | `/enrollment-review` | Professor | Approve/reject student enrollment requests |

### Design System

The UI is built on a custom **Material Design 3** inspired design system:

- **Typography:** Playfair Display (headings) + Hanken Grotesk (body)
- **Color Palette:** 30+ semantic color tokens (primary, secondary, tertiary, surface variants)
- **Animations:** GSAP scroll-triggered reveals, particle canvas background, geometric spin effects
- **Responsive:** Mobile-first design with breakpoints for tablet and desktop

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/) (for Tailwind CSS build)
- [PostgreSQL](https://www.postgresql.org/) or [Supabase account](https://supabase.com/)
- [AWS Account](https://aws.amazon.com/) with Rekognition access
- (Optional) [Redis](https://redis.io/) for production rate limiting

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/Attencial.git
   cd Attencial
   ```

2. **Configure environment variables**
   
   Create `Attencial.API/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=your-host;Port=5432;Database=postgres;Username=your-user;Password=your-password"
     },
     "JwtSettings": {
       "SecretKey": "your-secret-key-at-least-64-characters-long",
       "Issuer": "Attencial.API",
       "Audience": "Attencial.Client",
       "ExpiryMinutes": 60
     },
     "AwsRekognition": {
       "AccessKey": "YOUR_AWS_ACCESS_KEY",
       "SecretKey": "YOUR_AWS_SECRET_KEY",
       "Region": "us-east-1",
       "CollectionId": "attencial-students"
     },
     "ClientBaseUrl": "https://localhost:7251"
   }
   ```

3. **Install Tailwind CSS dependencies**
   ```bash
   cd Attencial.Client
   npm install
   ```

4. **Apply database migrations**
   ```bash
   cd Attencial.API
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   # From the solution root
   dotnet run --project Attencial.API
   ```
   
   The API serves both the backend and the Blazor WASM client. Open your browser to the URL shown in the terminal.

---

## ⚙️ Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `ConnectionStrings:DefaultConnection` | ✅ | PostgreSQL connection string |
| `ConnectionStrings:Redis` | ❌ | Redis connection string (falls back to in-memory cache) |
| `JwtSettings:SecretKey` | ✅ | HMAC-SHA256 signing key (≥64 chars recommended) |
| `JwtSettings:Issuer` | ✅ | JWT issuer claim |
| `JwtSettings:Audience` | ✅ | JWT audience claim |
| `JwtSettings:ExpiryMinutes` | ✅ | Token expiration time |
| `AwsRekognition:AccessKey` | ✅ | AWS IAM access key |
| `AwsRekognition:SecretKey` | ✅ | AWS IAM secret key |
| `AwsRekognition:Region` | ✅ | AWS region (e.g., `us-east-1`) |
| `AwsRekognition:CollectionId` | ✅ | Rekognition collection name |
| `Supabase:Url` | ❌ | Supabase project URL (for Realtime) |
| `Supabase:AnonKey` | ❌ | Supabase anon key (for Realtime) |
| `ClientBaseUrl` | ❌ | Frontend URL for QR code generation |
| `AllowedOrigins` | ❌ | CORS allowed origins array |

---

## 🌐 Deployment

The application is deployed and live at **[attencial.live](https://attencial.live)**.

### Architecture Overview

- **Hosting Environment**: Azure App Service
- **API + Client**: Hosted together — the ASP.NET Core API serves the Blazor WASM static files
- **Database**: Supabase (managed PostgreSQL) with connection pooling
- **Face Recognition**: AWS Rekognition (us-east-1)
- **Secrets & Configuration**: Loaded dynamically from Azure App Service Environment Variables

### Azure Deployment via Kudu Terminal

For maximum control and debugging during the deployment phase, the project was deployed directly using Azure's **Kudu Console / Advanced Tools**.

#### 1. Navigating to the Kudu Debug Console
1. Navigate to the Azure Portal, select the App Service instance, and search for **Advanced Tools** (or go directly to `https://attencial.scm.azurewebsites.net/DebugConsole`).
2. Open the command-line interface (CMD or PowerShell).

#### 2. Pulling the Latest Codebase
The repository is located in the repository directory where Azure tracks git history:
```bash
cd D:\home\site\repository
# Or on Linux: cd /home/site/repository

# Fetch the latest updates from GitHub
git pull origin main
```

#### 3. Building Frontend Assets (Tailwind CSS)
Because the Blazor client utilizes a compiled Tailwind CSS configuration, node packages must be restored and the CSS minified before publishing the App Service:
```bash
# Navigate to the Client project
cd Attencial.Client

# Install dependencies using clean install
npm ci

# Compile Tailwind utility classes
npm run build

# Navigate back to repository root
cd ..
```

#### 4. Compiling & Publishing the .NET Solution
Run the .NET CLI within the Kudu terminal to compile the code-behind Blazor pages, API controllers, and repositories, then output the release build directly into the App Service's webroot:
```bash
# Publish the API project (which hosts and serves the Client static assets)
dotnet publish Attencial.API/Attencial.API.csproj -c Release -o D:\home\site\wwwroot
# Or on Linux: dotnet publish Attencial.API/Attencial.API.csproj -c Release -o /home/site/wwwroot
```

#### 5. Configuration & Environment Variables
Azure App Service environment variables were configured via the Portal (under **Settings > Configuration**):
- `ConnectionStrings__DefaultConnection` — Supabase PostgreSQL Connection String
- `JwtSettings__SecretKey` — HMAC-SHA256 Token Signature Key
- `AwsRekognition__AccessKey` & `AwsRekognition__SecretKey` — AWS IAM credentials
- `AwsRekognition__CollectionId` — `attencial-students`
- `ClientBaseUrl` — `https://attencial.live`

---

## 🧠 Engineering Decisions

### Why Blazor WASM over React/Angular?

Blazor WebAssembly allows sharing C# models and DTOs between the API and client through the `Attencial.Shared` class library. This eliminates the need for duplicate TypeScript interfaces, reduces type mismatch bugs, and keeps the entire stack in a single language.

### Why AWS Rekognition over local face_recognition or Azure?

| Criteria | Local (dlib/face_recognition) | Azure Face API | AWS Rekognition ✅ |
|----------|------|------|------|
| Setup complexity | Requires Python + C++ deps | DNS/endpoint issues encountered | Simple SDK + API keys |
| Indexing speed | Manual vector storage | Training step required | Instant — IndexFaces returns immediately |
| Scalability | Limited by server CPU | Cloud-native | Cloud-native |
| Cost (prototype) | Free | Free tier issues | Free tier available |

During development, we initially integrated Azure Face API but encountered persistent DNS resolution failures and key format incompatibilities. AWS Rekognition's instant face indexing (no training step) and cleaner SDK made it the pragmatic choice.

### Why Redis with In-Memory Fallback?

Redis provides distributed, persistent rate limiting across API instances. However, requiring Redis for local development creates unnecessary friction. The `IDistributedCache` abstraction allows the system to:
- Use **Redis** in production for shared state across scaled instances
- Fall back to **DistributedMemoryCache** in development for zero-config startup

### Why Repository Pattern?

While EF Core's `DbContext` already acts as a Unit of Work, the Repository pattern provides:
- **Testability** — Mock `IStudentRepository` without touching the database
- **Encapsulation** — Complex queries are named methods, not inline LINQ in controllers
- **Consistency** — All data access follows the same pattern

---

<p align="center">
  Built with love using .NET 10, AWS Rekognition, and Supabase
</p>
