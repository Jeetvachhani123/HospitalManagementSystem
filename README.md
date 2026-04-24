# 🏥 Hospital Management System

<div align="center">

![.NET](https://img.shields.io/badge/.NET%209-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

A **full-stack, production-ready Hospital Management System** built with **ASP.NET Core (.NET 9)** following **Clean Architecture** principles. It features a RESTful Web API backend, an ASP.NET Core MVC frontend, JWT-based authentication, role-based access control, real-time notifications via SignalR, and rich data export capabilities.

</div>

---

## 📑 Table of Contents

- [Features](#-features)
- [Architecture Overview](#-architecture-overview)
- [Technology Stack](#-technology-stack)
- [Project Structure](#-project-structure)
- [Domain Entities & Database Schema](#-domain-entities--database-schema)
- [User Roles & Permissions](#-user-roles--permissions)
- [API Endpoints](#-api-endpoints)
- [Security](#-security)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [Author](#-author)

---

## ✨ Features

### Core Modules
- 👤 **Patient Management** — Registration, profile management, medical history, allergies
- 👨‍⚕️ **Doctor Management** — Profiles, specializations, availability, working-hours scheduling, appointment calendar
- 📅 **Appointment System** — Full lifecycle management with approval workflow (request → approve/reject → complete)
- 🏢 **Department Management** — Add, update, and organize hospital departments
- 🗂️ **Medical Records** — Doctor-created consultation records with patient-access control
- 💳 **Billing & Invoicing** — Invoice generation, Stripe payment integration, pending-invoice tracking

### Advanced Features
- 🔔 **Real-Time Notifications** — SignalR hub for live appointment status updates
- 📧 **Email Notifications** — Automated emails on appointment events (request, approval, rejection, completion)
- 📊 **Reporting & Analytics** — Dashboard stats, appointment reports, doctor performance, monthly trends, full report snapshots
- 📤 **Data Export** — Export appointments to **CSV**, **Excel** (ClosedXML), and **PDF** (iText7)
- 🔍 **Advanced Search & Pagination** — Searchable appointment and doctor listings
- ⚡ **Rate Limiting** — Fixed-window rate limiter (100 req/min per user/IP)
- 🩺 **Health Checks** — `/health` endpoint with SQL Server connectivity check
- 📋 **Swagger / OpenAPI** — Interactive API documentation for development
- 🗃️ **Caching** — In-memory distributed cache service
- ✅ **FluentValidation** — Input validation with rich, descriptive error messages
- 🗺️ **AutoMapper** — DTO↔Entity mapping with a centralized profile
- 📝 **Serilog Logging** — Structured logs to console and rolling daily log files

---

## 🏛️ Architecture Overview

The solution follows **Clean Architecture** with strict dependency rules:

```
┌───────────────────────────────────────────────────────────────┐
│  Presentation Layer                                           │
│  ┌─────────────────────┐   ┌─────────────────────────────┐   │
│  │   HospitalMS.API    │   │      HospitalMS.Web         │   │
│  │  (REST Web API)     │   │  (ASP.NET Core MVC)         │   │
│  └──────────┬──────────┘   └─────────────┬───────────────┘   │
└─────────────┼────────────────────────────┼───────────────────┘
              │                            │
┌─────────────▼────────────────────────────▼───────────────────┐
│  Business Logic Layer                                         │
│              HospitalMS.BL                                    │
│  Services │ DTOs │ Interfaces │ Validators │ AutoMapper       │
└──────────────────────────┬───────────────────────────────────┘
                           │
┌──────────────────────────▼───────────────────────────────────┐
│  Data Access Layer                                            │
│              HospitalMS.DATA                                  │
│  EF Core DbContext │ Repositories │ Unit of Work │ Migrations │
└──────────────────────────┬───────────────────────────────────┘
                           │
┌──────────────────────────▼───────────────────────────────────┐
│  Domain Layer                                                 │
│              HospitalMS.Models                                │
│  Entities │ Enums │ Value Objects │ Base Classes              │
└──────────────────────────────────────────────────────────────┘
```

**Dependency Rule:** each layer depends only on the layer below it. `HospitalMS.Models` has zero external dependencies.

---

## 🛠️ Technology Stack

| Category | Technology |
|---|---|
| **Runtime** | .NET 9 / C# 13 |
| **Web API** | ASP.NET Core Web API |
| **MVC Frontend** | ASP.NET Core MVC (Razor Views) |
| **ORM** | Entity Framework Core 9 |
| **Database** | Microsoft SQL Server |
| **Authentication** | JWT Bearer Tokens (API) + Cookie Auth (Web) |
| **Real-Time** | SignalR (`NotificationHub`) |
| **Validation** | FluentValidation |
| **Mapping** | AutoMapper |
| **Logging** | Serilog (Console + Rolling File) |
| **API Versioning** | `Asp.Versioning.Mvc` |
| **Rate Limiting** | ASP.NET Core built-in `RateLimiter` |
| **Health Checks** | `Microsoft.AspNetCore.Diagnostics.HealthChecks` + SQL Server |
| **PDF Export** | iText7 |
| **Excel Export** | ClosedXML |
| **Payments** | Stripe |
| **API Docs** | Swagger / OpenAPI |
| **Caching** | Distributed In-Memory Cache |

---

## 📂 Project Structure

```
HospitalManagementSystem/
│
├── HospitalMS.API/                     # REST API (Presentation)
│   ├── Controllers/
│   │   ├── AuthController.cs           # Login, Register, Token validation
│   │   ├── AppointmentsController.cs   # Full CRUD + Export (CSV/Excel/PDF)
│   │   ├── AppointmentWorkflowController.cs  # Approve/Reject/Complete/Reschedule workflow
│   │   ├── DoctorsController.cs        # Doctor CRUD + Calendar + Availability
│   │   ├── PatientsController.cs       # Patient CRUD
│   │   ├── DepartmentsController.cs    # Department CRUD
│   │   ├── MedicalRecordsController.cs # Medical record management
│   │   ├── BillingController.cs        # Invoices + Payment processing
│   │   ├── ReportsController.cs        # Dashboards + Analytics + Reports
│   │   └── WorkingHoursController.cs   # Doctor schedule management
│   ├── Extensions/                     # Service registration extensions
│   ├── Filters/                        # GlobalExceptionFilter, ValidateModelFilter
│   ├── Hubs/                           # SignalR NotificationHub
│   ├── Middlewares/                    # ExceptionMiddleware
│   ├── Services/                       # CurrentUserService
│   ├── appsettings.json                # App configuration
│   └── Program.cs                      # Startup & DI configuration
│
├── HospitalMS.BL/                      # Business Logic Layer
│   ├── Services/                       # 18 service implementations
│   │   ├── AppointmentService.cs
│   │   ├── AppointmentWorkflowService.cs
│   │   ├── AuthService.cs
│   │   ├── BillingService.cs
│   │   ├── CacheService.cs
│   │   ├── DepartmentService.cs
│   │   ├── DoctorService.cs
│   │   ├── EmailNotificationService.cs
│   │   ├── EmailService.cs
│   │   ├── ExportService.cs
│   │   ├── MedicalRecordService.cs
│   │   ├── PaginationService.cs
│   │   ├── PatientService.cs
│   │   ├── ReportingService.cs
│   │   ├── SearchService.cs
│   │   ├── StripePaymentService.cs
│   │   ├── UserRegistrationCoordinator.cs
│   │   └── WorkingHoursService.cs
│   ├── DTOs/                           # Data Transfer Objects (Appointment, Auth, Doctor, Patient, etc.)
│   ├── Interfaces/                     # Service & Repository contracts
│   ├── Validators/                     # FluentValidation validators (13 files)
│   ├── Mappings/                       # AutoMapper profile
│   ├── Common/                         # ApiResponse<T>, JwtSettings, Constants
│   └── Exceptions/                     # Custom exception types
│
├── HospitalMS.DATA/                    # Data Access Layer
│   ├── Context/
│   │   └── HospitalDbContext.cs        # EF Core DbContext
│   ├── Repositories/                   # 9 repository implementations
│   │   ├── AppointmentRepository.cs
│   │   ├── AppointmentStatusHistoryRepository.cs
│   │   ├── DepartmentRepository.cs
│   │   ├── DoctorRepository.cs
│   │   ├── DoctorWorkingHoursRepository.cs
│   │   ├── InvoiceRepository.cs
│   │   ├── MedicalRecordRepository.cs
│   │   ├── PatientRepository.cs
│   │   └── UserRepository.cs
│   ├── UnitOfWork/                     # Unit of Work pattern
│   ├── Configurations/                 # EF Fluent API entity configurations
│   ├── Migrations/                     # EF Core database migrations
│   └── ServiceCollectionExtensions.cs
│
├── HospitalMS.Models/                  # Domain Layer (no dependencies)
│   ├── Entities/
│   │   ├── Appointment.cs
│   │   ├── AppointmentStatusHistory.cs
│   │   ├── Department.cs
│   │   ├── Doctor.cs
│   │   ├── DoctorWorkingHours.cs
│   │   ├── Invoice.cs
│   │   ├── MedicalRecord.cs
│   │   ├── Patient.cs
│   │   ├── RescheduleRequest.cs
│   │   └── User.cs
│   ├── Enums/
│   │   ├── AppointmentStatus.cs        # Scheduled, Confirmed, Completed, Cancelled, NoShow
│   │   ├── AppointmentApprovalStatus.cs # Pending, Approved, Rejected
│   │   └── UserRole.cs                 # Admin, Doctor, Patient
│   ├── Base/                           # AuditableEntity base class
│   └── ValueObjects/                   # Address, etc.
│
└── HospitalMS.Web/                     # ASP.NET Core MVC Frontend
    ├── Controllers/                    # 9 MVC controllers
    │   ├── AccountController.cs        # Login/Register/Logout (Cookie auth)
    │   ├── AdminController.cs          # Admin dashboard
    │   ├── AppointmentController.cs    # Full appointment UI
    │   ├── BillingController.cs        # Billing & invoice views
    │   ├── DepartmentController.cs
    │   ├── DoctorController.cs
    │   ├── MedicalRecordController.cs
    │   ├── PatientController.cs
    │   └── HomeController.cs
    ├── Views/                          # Razor views for all modules
    ├── ViewModels/                     # View-specific models
    ├── Hubs/                           # SignalR hub for MVC
    ├── Services/                       # ApiClient, CurrentUserService
    ├── Middleware/                     # Security headers, rate limiting, logging
    └── Program.cs                      # Startup & DI configuration
```

---

## 🗃️ Domain Entities & Database Schema

| Entity | Key Fields |
|---|---|
| **User** | Id, FirstName, LastName, Email, PasswordHash, Role |
| **Patient** | UserId, DateOfBirth, BloodGroup, Gender, Address, EmergencyContact, MedicalHistory, Allergies |
| **Doctor** | UserId, Specialization, LicenseNumber, YearsOfExperience, PhoneNumber, IsAvailable |
| **Department** | Id, Name, Description |
| **DoctorWorkingHours** | DoctorId, DayOfWeek, StartTime, EndTime, IsWorkingDay |
| **Appointment** | PatientId, DoctorId, Date, StartTime, EndTime, Status, ApprovalStatus, Diagnosis, Prescription, Notes, IsRescheduled |
| **AppointmentStatusHistory** | AppointmentId, OldStatus, NewStatus, ChangedAt, ChangedByUserId, Reason |
| **RescheduleRequest** | AppointmentId, RequestedById, OldDate, NewDate, NewStartTime, NewEndTime |
| **MedicalRecord** | AppointmentId, PatientId, DoctorId, Diagnosis, Prescription, Notes, FollowUpDate |
| **Invoice** | PatientId, AppointmentId, Amount, IsPaid, PaymentMethod, PaidAt |

---

## 👥 User Roles & Permissions

| Feature | Admin | Doctor | Patient |
|---|:---:|:---:|:---:|
| View all appointments | ✅ | ✅ | ❌ (own only) |
| Create appointment | ✅ | ✅ | ✅ |
| Approve / Reject appointment | ❌ | ✅ | ❌ |
| Complete appointment | ❌ | ✅ | ❌ |
| Cancel appointment | ✅ | ✅ | ✅ (own) |
| Reschedule appointment | ✅ | ✅ | ✅ (own) |
| Mark no-show | ✅ | ✅ | ❌ |
| Manage doctors (CRUD) | ✅ | ❌ | ❌ |
| Update own doctor profile | ❌ | ✅ | ❌ |
| Manage working hours | ✅ | ✅ (own) | ❌ |
| View medical records | ✅ | ✅ (own) | ✅ (own) |
| Create medical records | ❌ | ✅ | ❌ |
| View billing / invoices | ✅ | ❌ | ✅ (own) |
| Process payment | ❌ | ❌ | ✅ |
| Cancel invoice | ✅ | ❌ | ❌ |
| Export data (CSV/Excel/PDF) | ✅ | ✅ | ✅ |
| View admin dashboard | ✅ | ❌ | ❌ |
| View doctor dashboard | ❌ | ✅ | ❌ |
| View patient dashboard | ❌ | ❌ | ✅ |
| Manage departments | ✅ | ❌ | ❌ |

---

## 📡 API Endpoints

### 🔐 Authentication — `/api/auth`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/login` | Public | Login and receive JWT token |
| POST | `/api/auth/register` | Public | Register a new user |
| POST | `/api/auth/validate` | Public | Validate an existing JWT token |

---

### 📅 Appointments — `/api/appointments`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/appointments` | Any | Get all (role-filtered) |
| GET | `/api/appointments/{id}` | Any | Get by ID (ownership check) |
| GET | `/api/appointments/patient/{patientId}` | Any | Get by patient |
| GET | `/api/appointments/doctor/{doctorId}` | Any | Get by doctor |
| GET | `/api/appointments/date-range` | Any | Filter by date range |
| GET | `/api/appointments/status/{status}` | Any | Filter by status |
| GET | `/api/appointments/my` | Any | Get current user's appointments |
| GET | `/api/appointments/upcoming` | Any | Upcoming appointments |
| GET | `/api/appointments/history` | Any | Past appointments |
| POST | `/api/appointments` | Any | Create appointment |
| POST | `/api/appointments/check-conflict` | Any | Check scheduling conflict |
| PUT | `/api/appointments/{id}` | Any | Update appointment |
| PATCH | `/api/appointments/{id}/status` | Admin, Doctor | Update appointment status |
| DELETE | `/api/appointments/{id}` | Any | Cancel appointment |
| GET | `/api/appointments/export/csv` | Any | Export to CSV |
| GET | `/api/appointments/export/excel` | Any | Export to Excel |
| GET | `/api/appointments/export/pdf` | Any | Export to PDF |

---

### 🔄 Appointment Workflow — `/api/appointmentworkflow`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/appointmentworkflow/request` | Patient | Request a new appointment |
| POST | `/api/appointmentworkflow/{id}/approve` | Doctor | Approve a pending appointment |
| POST | `/api/appointmentworkflow/{id}/reject` | Doctor | Reject a pending appointment |
| POST | `/api/appointmentworkflow/{id}/complete` | Doctor | Mark as completed with notes |
| POST | `/api/appointmentworkflow/{id}/cancel` | Any | Cancel appointment |
| POST | `/api/appointmentworkflow/{id}/reschedule` | Any | Reschedule appointment |
| POST | `/api/appointmentworkflow/{id}/no-show` | Doctor, Admin | Mark as no-show |
| GET | `/api/appointmentworkflow/pending-approvals` | Doctor | Get pending approvals |
| GET | `/api/appointmentworkflow/doctors/{id}/available-slots` | Public | Get available time slots |
| GET | `/api/appointmentworkflow/{id}/status-history` | Any | Get status change history |

---

### 👨‍⚕️ Doctors — `/api/doctors`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/doctors` | Any | List all doctors (paginated) |
| GET | `/api/doctors/{id}` | Any | Get doctor by ID |
| GET | `/api/doctors/user/{userId}` | Any | Get doctor by user ID |
| GET | `/api/doctors/{id}/calendar` | Any | Get monthly appointment calendar |
| GET | `/api/doctors/available` | Any | List available doctors |
| GET | `/api/doctors/specialization/{spec}` | Any | Filter by specialization |
| POST | `/api/doctors` | Admin | Create doctor |
| PUT | `/api/doctors/{id}` | Admin, Doctor | Update doctor profile |
| DELETE | `/api/doctors/{id}` | Admin | Delete doctor |

---

### 👤 Patients — `/api/patients`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/patients` | Admin, Doctor | List all patients (paginated) |
| GET | `/api/patients/{id}` | Any | Get patient (ownership check) |
| GET | `/api/patients/user/{userId}` | Any | Get patient by user ID |
| POST | `/api/patients` | Public | Register patient |
| DELETE | `/api/patients/{id}` | Admin | Delete patient |

---

### 🏢 Departments — `/api/departments`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/departments` | Public | List all departments |
| GET | `/api/departments/{id}` | Public | Get department by ID |
| POST | `/api/departments` | Admin | Create department |
| PUT | `/api/departments/{id}` | Admin | Update department |
| DELETE | `/api/departments/{id}` | Admin | Delete department |

---

### 🗂️ Medical Records — `/api/medicalrecords`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/medicalrecords/patient/{patientId}` | Any | Records by patient |
| GET | `/api/medicalrecords/my-records` | Patient | Own records |
| GET | `/api/medicalrecords/doctor/{doctorId}` | Doctor, Admin | Records by doctor |
| GET | `/api/medicalrecords/my-created-records` | Doctor | Records created by doctor |
| GET | `/api/medicalrecords/{id}` | Any | Get record by ID |
| POST | `/api/medicalrecords` | Doctor | Create medical record |
| DELETE | `/api/medicalrecords/{id}` | Doctor, Admin | Delete medical record |

---

### 💳 Billing — `/api/billing`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/billing` | Admin | List all invoices |
| GET | `/api/billing/{id}` | Any | Get invoice by ID |
| GET | `/api/billing/patient/{patientId}` | Any | Invoices by patient |
| GET | `/api/billing/my-invoices` | Patient | Own invoices |
| GET | `/api/billing/pending` | Patient | Pending invoices |
| POST | `/api/billing/{id}/pay` | Patient | Process payment (Stripe) |
| DELETE | `/api/billing/{id}` | Admin | Cancel invoice |

---

### 📊 Reports & Dashboard — `/api/reports`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/reports/appointments` | Admin | Full appointment report |
| GET | `/api/reports/doctor/{doctorId}` | Doctor, Admin | Doctor performance report |
| GET | `/api/reports/my-performance` | Doctor | Own performance report |
| GET | `/api/reports/system-stats` | Admin | System-wide statistics |
| GET | `/api/reports/monthly-trend` | Admin | Monthly trend (1–24 months) |
| GET | `/api/reports/dashboard` | Any | Role-based dashboard stats |
| GET | `/api/reports/dashboard/admin` | Admin | Admin dashboard data |
| GET | `/api/reports/dashboard/doctor` | Doctor | Doctor dashboard data |
| GET | `/api/reports/dashboard/patient` | Patient | Patient dashboard data |
| GET | `/api/reports/cards` | Admin | Quick card data (Doctors/Patients/Today/Pending) |
| GET | `/api/reports/full` | Admin | Full snapshot report |

---

### ⏰ Working Hours — `/api/doctors/{doctorId}/working-hours`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/doctors/{doctorId}/working-hours` | Public | Get doctor's working hours |
| PUT | `/api/doctors/{doctorId}/working-hours` | Doctor, Admin | Update working hours |
| GET | `/api/my-working-hours` | Doctor | Get own working hours |
| PUT | `/api/my-working-hours` | Doctor | Update own working hours |

---

## 🔐 Security

- **JWT Authentication** — Issued on login, validated on every API request; configurable expiry (default 1440 min)
- **Cookie Authentication** — Used by the MVC frontend (`HttpOnly`, `Secure`, `SameSite=Strict`)
- **Role-Based Authorization** — `[Authorize(Roles = "...")]` applied per endpoint
- **Ownership Validation** — Patients can only access their own data; doctors can only manage their own schedules/records
- **FluentValidation** — Input validation before data reaches the service layer
- **Rate Limiting** — 100 requests/minute per user or IP
- **CORS** — Restricted to known origins
- **CSRF Protection** — Anti-forgery tokens on MVC forms
- **Security Headers** — Custom `SecurityHeadersMiddleware` in the Web project
- **Global Exception Handling** — Centralized `ExceptionMiddleware` / `GlobalExceptionFilter`
- **Serilog Request Logging** — All requests logged for audit trail
- **Optimistic Concurrency** — `[Timestamp] RowVersion` on `Appointment` and `Patient` entities

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (Express or full edition)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) or VS Code
- *(Optional)* Stripe account for payment features

### Setup Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/Jeetvachhani123/HospitalManagementSystem.git
   cd HospitalManagementSystem
   ```

2. **Update connection strings**

   In `HospitalMS.API/appsettings.json` and `HospitalMS.Web/appsettings.json`, set your SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=HospitalMS_DB;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. **Apply database migrations**
   ```bash
   cd HospitalMS.API
   dotnet ef database update --project ../HospitalMS.DATA
   ```

4. **Configure JWT settings** *(in `appsettings.json`)*
   ```json
   "JwtSettings": {
     "Secret": "YOUR_STRONG_SECRET_KEY_MIN_32_CHARS",
     "Issuer": "HospitalMS.API",
     "Audience": "HospitalMS.Web",
     "ExpirationInMinutes": 1440
   }
   ```

5. **Run the projects**

   Run both projects simultaneously (API first, then Web):
   ```bash
   # Terminal 1 — API
   cd HospitalMS.API && dotnet run

   # Terminal 2 — Web
   cd HospitalMS.Web && dotnet run
   ```

   Or use Visual Studio with **Multiple Startup Projects** configured.

6. **Access the application**

   | Project | URL |
   |---|---|
   | REST API (Swagger) | `https://localhost:7058/swagger` |
   | Health Check | `https://localhost:7058/health` |
   | MVC Web App | `https://localhost:XXXX` |

---

## ⚙️ Configuration

| Key | Location | Purpose |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | appsettings.json | SQL Server connection |
| `ConnectionStrings:Redis` | appsettings.json | Redis (optional, currently uses in-memory) |
| `JwtSettings:Secret` | appsettings.json | JWT signing key (min 32 chars) |
| `JwtSettings:ExpirationInMinutes` | appsettings.json | Token TTL |
| `AppointmentSettings` | appsettings.json | Booking rules (advance days, slot duration, etc.) |
| `ApiSettings:BaseUrl` | Web/appsettings.json | API base URL for MVC ↔ API communication |
| `Serilog` | appsettings.json | Log levels and sinks (Console + File) |

---

## 👨‍💻 Author

**Jeet Vachhani**

- 🎓 BCA Graduate
- 💼 .NET Developer
- 🛠️ Skills: C#, ASP.NET Core, Entity Framework Core, SQL Server, Clean Architecture, MVC, HTML, CSS, SignalR, REST APIs

---

## 📬 Contact

Feel free to connect on **LinkedIn** for collaboration and opportunities.

---

<div align="center">

⭐ **If you found this project helpful, please give it a star!** ⭐

</div>
