# 🏥 Hospital Management System

<div align="center">

![.NET](https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Entity Framework](https://img.shields.io/badge/EF%20Core%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Stripe](https://img.shields.io/badge/Stripe-008FE2?style=for-the-badge&logo=stripe&logoColor=white)

**A full-stack, enterprise-grade Hospital Management System built with ASP.NET Core (.NET 8), following Clean Architecture principles.**

It features a RESTful Web API backend, an ASP.NET Core MVC frontend with Bootstrap UI, JWT + Cookie-based dual authentication, real-time SignalR notifications, Stripe payment integration, and rich data export capabilities (CSV, Excel, PDF).

</div>

---

## 📑 Table of Contents

- [Architecture Overview](#-architecture-overview)
- [Technology Stack](#-technology-stack)
- [Solution Structure](#-solution-structure)
- [Domain Model](#-domain-model)
- [System Features & Modules](#-system-features--modules)
- [User Roles & Permissions](#-user-roles--permissions)
- [Appointment Lifecycle](#-appointment-lifecycle)
- [Billing & Stripe Workflow](#-billing--stripe-workflow)
- [API Endpoints Reference](#-api-endpoints-reference)
- [Security Implementation](#-security-implementation)
- [Configuration Reference](#-configuration-reference)
- [Getting Started](#-getting-started)

---

## 🏛️ Architecture Overview

This solution implements **Clean Architecture** with strict unidirectional dependency rules. Each layer depends only on the layer directly below it. `HospitalMS.Models` (the Domain) has **zero external dependencies**.

```
┌──────────────────────────────────────────────────────────┐
│                   Presentation Layer                     │
│  ┌─────────────────────┐   ┌──────────────────────────┐  │
│  │   HospitalMS.API    │   │     HospitalMS.Web       │  │
│  │  (REST Web API)     │   │  (ASP.NET Core MVC)      │  │
│  │  JWT Auth           │   │  Cookie Auth             │  │
│  │  Rate Limiting      │   │  Anti-Forgery (CSRF)     │  │
│  │  Swagger / OpenAPI  │   │  Security Headers        │  │
│  └──────────┬──────────┘   └────────────┬─────────────┘  │
└─────────────┼───────────────────────────┼────────────────┘
              │                           │
┌─────────────▼───────────────────────────▼────────────────┐
│              Business Logic Layer (HospitalMS.BL)        │
│   18 Services │ DTOs │ Interfaces │ 13 Validators        │
│   AutoMapper Profile │ FluentValidation │ Email/Notify   │
└──────────────────────────┬───────────────────────────────┘
                           │
┌──────────────────────────▼───────────────────────────────┐
│            Data Access Layer (HospitalMS.DATA)           │
│   EF Core DbContext │ 9 Repositories │ Unit of Work      │
│   Fluent Configurations │ EF Migrations                  │
└──────────────────────────┬───────────────────────────────┘
                           │
┌──────────────────────────▼───────────────────────────────┐
│              Domain Layer (HospitalMS.Models)            │
│   10 Entities │ 3 Enums │ AuditableEntity Base           │
│   Value Objects (Address) │ No External Dependencies     │
└──────────────────────────────────────────────────────────┘
```

---

## 🛠️ Technology Stack

| Category | Technology | Purpose |
|---|---|---|
| **Runtime** | .NET 8 / C# 12 | Core framework |
| **Web API** | ASP.NET Core Web API | RESTful backend |
| **Frontend** | ASP.NET Core MVC (Razor Views) | Server-rendered UI |
| **ORM** | Entity Framework Core 8 | Database access |
| **Database** | Microsoft SQL Server | Data persistence |
| **Authentication** | JWT Bearer (API) + Cookie Auth (Web) | Dual-layer auth |
| **Password Hashing** | BCrypt | Secure credential storage |
| **Real-Time** | SignalR | Live appointment notifications |
| **Input Validation** | FluentValidation | Rich, rule-based input checks |
| **Object Mapping** | AutoMapper | DTO ↔ Entity mapping |
| **Logging** | Serilog | Structured logging to console & rolling files |
| **API Versioning** | Asp.Versioning.Mvc | Versioned API endpoints |
| **Rate Limiting** | ASP.NET Core built-in RateLimiter | Brute-force & DoS protection |
| **Health Checks** | Microsoft.AspNetCore.Diagnostics.HealthChecks | SQL Server liveness probe |
| **PDF Export** | QuestPDF | Styled landscape PDF reports |
| **Excel Export** | ClosedXML | Formatted .xlsx workbooks |
| **CSV Export** | System.Text (custom, injection-safe) | UTF-8 BOM CSV with formula injection protection |
| **Payments** | Stripe API + Webhooks | PCI-compliant payment processing |
| **API Docs** | Swagger / OpenAPI | Interactive API explorer |
| **Caching** | Distributed In-Memory Cache | Lightweight response caching |

---

## 📂 Solution Structure

```
HospitalManagementSystem/
│
├── HospitalMS.Models/                  ← Domain Layer (zero dependencies)
│   ├── Entities/
│   │   ├── User.cs                     ← Auth identity (Email, PasswordHash, Role)
│   │   ├── Patient.cs                  ← Profile + DOB, BloodGroup, Allergies, Address (Value Object)
│   │   ├── Doctor.cs                   ← Specialization, Fee, DepartmentId, WorkingHours
│   │   ├── Department.cs               ← Hospital departments
│   │   ├── Appointment.cs              ← Core entity with Status, ApprovalStatus, RowVersion (optimistic concurrency)
│   │   ├── AppointmentStatusHistory.cs ← Immutable audit trail of every status change
│   │   ├── RescheduleRequest.cs        ← Tracks old/new dates when an appointment is rescheduled
│   │   ├── DoctorWorkingHours.cs       ← Per-day availability schedule for each doctor
│   │   ├── MedicalRecord.cs            ← Diagnosis, Prescription, Notes, FollowUpDate
│   │   └── Invoice.cs                  ← Amount, IsPaid, PaymentMethod, StripeSessionId
│   ├── Enums/
│   │   ├── AppointmentStatus.cs        ← Scheduled | Confirmed | Completed | Cancelled | NoShow
│   │   ├── AppointmentApprovalStatus.cs← Pending | Approved | Rejected
│   │   └── UserRole.cs                 ← Admin | Doctor | Patient
│   ├── Base/
│   │   └── AuditableEntity.cs          ← Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
│   └── ValueObjects/
│       └── Address.cs
│
├── HospitalMS.DATA/                    ← Data Access Layer
│   ├── Context/
│   │   └── HospitalDbContext.cs        ← EF Core DbContext with all DbSets
│   ├── Repositories/                   ← 9 concrete repositories
│   │   ├── AppointmentRepository.cs    ← Rich queries: SearchAsync, GetByDateRange, GetUpcoming
│   │   ├── DoctorRepository.cs         ← GetAvailable, GetBySpecialization, GetWithWorkingHours
│   │   ├── PatientRepository.cs        ← GetByUserId, GetWithAppointments
│   │   ├── InvoiceRepository.cs        ← GetPending, GetByPatient, GetByStripeSession
│   │   ├── MedicalRecordRepository.cs  ← GetByPatient, GetByDoctor
│   │   ├── AppointmentStatusHistoryRepository.cs
│   │   ├── DoctorWorkingHoursRepository.cs
│   │   ├── DepartmentRepository.cs
│   │   └── UserRepository.cs
│   ├── UnitOfWork/                     ← Wraps all repositories; single SaveChangesAsync call
│   ├── Configurations/                 ← EF Fluent API entity configurations
│   └── Migrations/                     ← EF Core generated database migrations
│
├── HospitalMS.BL/                      ← Business Logic Layer
│   ├── Services/                       ← 18 service implementations
│   │   ├── AppointmentService.cs       ← CRUD, Search, Conflict detection, Pagination
│   │   ├── AppointmentWorkflowService.cs ← Full lifecycle: Request→Approve→Complete→Cancel→NoShow
│   │   ├── AuthService.cs              ← Login, Register, JWT generation, token validation
│   │   ├── BillingService.cs           ← Invoice creation, retrieval, cancellation
│   │   ├── StripePaymentService.cs     ← Stripe Checkout session creation + Webhook processing
│   │   ├── MedicalRecordService.cs     ← Create, retrieve, delete records
│   │   ├── DoctorService.cs            ← Doctor CRUD + calendar + availability
│   │   ├── PatientService.cs           ← Patient CRUD + profile management
│   │   ├── DepartmentService.cs        ← Department CRUD
│   │   ├── WorkingHoursService.cs      ← Doctor schedule management
│   │   ├── ReportingService.cs         ← Appointment reports, doctor performance, monthly trends
│   │   ├── ExportService.cs            ← CSV (formula-injection safe), Excel (ClosedXML), PDF (QuestPDF)
│   │   ├── SearchService.cs            ← Cross-entity full-text search
│   │   ├── EmailService.cs             ← SMTP email sender
│   │   ├── EmailNotificationService.cs ← Sends emails on appointment events
│   │   ├── CacheService.cs             ← Distributed in-memory cache wrapper
│   │   ├── PaginationService.cs        ← Generic pagination helper
│   │   └── UserRegistrationCoordinator.cs ← Orchestrates User + Patient/Doctor creation
│   ├── DTOs/                           ← Request/Response DTOs (Appointment, Auth, Doctor, Patient, Billing, Reports)
│   ├── Validators/                     ← 13 FluentValidation validators
│   ├── Mappings/                       ← AutoMapper profile (centralized DTO ↔ Entity maps)
│   ├── Common/                         ← ApiResponse<T>, JwtSettings, AppointmentSettings, Constants
│   └── Exceptions/                     ← Custom exception types
│
├── HospitalMS.API/                     ← REST API (Presentation Layer)
│   ├── Controllers/                    ← 10 API controllers
│   │   ├── AuthController.cs           ← POST /login, /register, /validate
│   │   ├── AppointmentsController.cs   ← Full appointment CRUD + CSV/Excel/PDF export
│   │   ├── AppointmentWorkflowController.cs ← Request, Approve, Reject, Complete, Cancel, Reschedule, NoShow
│   │   ├── DoctorsController.cs        ← Doctor CRUD + calendar + availability
│   │   ├── PatientsController.cs       ← Patient CRUD
│   │   ├── DepartmentsController.cs    ← Department CRUD
│   │   ├── MedicalRecordsController.cs ← Medical record management
│   │   ├── BillingController.cs        ← Invoice retrieval + Stripe checkout + webhook
│   │   ├── ReportsController.cs        ← Dashboards, analytics, full report snapshots
│   │   └── WorkingHoursController.cs   ← Doctor working hours management
│   ├── Extensions/                     ← AddJwtAuthentication, AddSwaggerDocumentation, AddHospitalServices
│   ├── Filters/                        ← ValidateModelFilter, GlobalExceptionFilter
│   ├── Hubs/                           ← SignalR NotificationHub
│   ├── Middlewares/                    ← ExceptionMiddleware
│   ├── Services/                       ← CurrentUserService (reads JWT claims)
│   └── Program.cs                      ← DI container, middleware pipeline, rate limiting config
│
└── HospitalMS.Web/                     ← MVC Frontend (Presentation Layer)
    ├── Controllers/                    ← 9 MVC controllers
    │   ├── AccountController.cs        ← Login, Register, Logout (Cookie auth)
    │   ├── AdminController.cs          ← Admin dashboard + system stats
    │   ├── AppointmentController.cs    ← Book, View, Cancel, Reschedule, Export UI
    │   ├── DoctorController.cs         ← Doctor management + calendar UI
    │   ├── PatientController.cs        ← Patient management UI
    │   ├── MedicalRecordController.cs  ← Create + view medical records
    │   ├── BillingController.cs        ← Invoice list + Stripe payment UI
    │   ├── DepartmentController.cs     ← Department management UI
    │   └── HomeController.cs           ← Landing page
    ├── Views/                          ← Razor views for all modules
    ├── ViewModels/                     ← View-specific models
    ├── Hubs/                           ← SignalR hub (MVC)
    ├── Services/                       ← ApiClient, CurrentUserService (reads cookie claims)
    ├── Middleware/                     ← GlobalExceptionHandlerMiddleware, RateLimitingMiddleware,
    │                                      RequestLoggingMiddleware, SecurityHeadersMiddleware
    └── Program.cs                      ← Cookie auth, anti-forgery, session, DI config
```

---

## 🗃️ Domain Model

| Entity | Key Fields |
|---|---|
| **User** | `Id`, `FirstName`, `LastName`, `Email`, `PasswordHash` (BCrypt), `Role` |
| **Patient** | `UserId`, `DateOfBirth`, `BloodGroup`, `Gender`, `Address` (Value Object), `EmergencyContact`, `MedicalHistory`, `Allergies`, `RowVersion` |
| **Doctor** | `UserId`, `Specialization`, `LicenseNumber`, `YearsOfExperience`, `Qualifications`, `Bio`, `ConsultationFee`, `IsAvailable`, `AppointmentSlotDurationMinutes`, `DepartmentId`, `RowVersion` |
| **Department** | `Id`, `Name`, `Description` |
| **DoctorWorkingHours** | `DoctorId`, `DayOfWeek`, `StartTime`, `EndTime`, `IsWorkingDay` |
| **Appointment** | `PatientId`, `DoctorId`, `AppointmentDate`, `StartTime`, `EndTime`, `Status` (enum), `ApprovalStatus` (enum), `ApprovedByDoctorId`, `Reason`, `Diagnosis`, `Prescription`, `Notes`, `IsRescheduled`, `OriginalAppointmentId`, `RowVersion` |
| **AppointmentStatusHistory** | `AppointmentId`, `OldStatus`, `NewStatus`, `ChangedAt`, `ChangedByUserId`, `Reason` |
| **RescheduleRequest** | `AppointmentId`, `RequestedById`, `OldDate`, `NewDate`, `NewStartTime`, `NewEndTime` |
| **MedicalRecord** | `AppointmentId`, `PatientId`, `DoctorId`, `Diagnosis`, `Prescription`, `Notes`, `FollowUpDate` |
| **Invoice** | `PatientId`, `AppointmentId`, `Amount`, `IsPaid`, `PaymentMethod`, `StripeSessionId`, `PaidAt` |

> **Optimistic Concurrency:** `Appointment`, `Patient`, and `Doctor` entities use `[Timestamp] byte[] RowVersion` to prevent lost updates in concurrent edit scenarios.

---

## ✨ System Features & Modules

### 👤 Authentication & Identity
- **Register / Login** with BCrypt-hashed passwords and JWT token issuance.
- **Dual authentication:** JWT Bearer tokens for API, `HttpOnly Secure SameSite=Strict` Cookies for the Web app.
- **`UserRegistrationCoordinator`** atomically creates the `User` + the appropriate `Patient` or `Doctor` profile in a single transaction.

### 📅 Appointment Management
- **Smart scheduling** based on doctor's working hours and configured slot duration (`AppointmentSlotDurationMinutes`).
- **Conflict detection** prevents double-booking the same doctor in overlapping time slots.
- **Full lifecycle:** Request → Approve / Reject → Complete / Cancel / NoShow / Reschedule.
- **Status History** — every status change is recorded in `AppointmentStatusHistory` for a complete audit trail.
- **Configurable booking rules** via `AppointmentSettings` (max advance booking days, min cancellation hours, max per day).

### 🗂️ Medical Records
- Doctors create consultation records (Diagnosis, Prescription, Notes, Follow-up date) against completed appointments.
- Patients can view their own records; doctors can view records they created.

### 💳 Billing & Payments
- **Automated invoice creation** when a doctor marks an appointment as `Completed`.
- **Stripe Checkout integration** — a Stripe Session URL is generated; the patient is redirected to a hosted Stripe payment page.
- **Webhook handler** at `POST /api/billing/webhook` processes `checkout.session.completed` events from Stripe to update invoice status asynchronously.

### 📊 Dashboards & Reporting
- **Admin Dashboard:** Total doctors, patients, today's appointments, pending approvals, revenue.
- **Doctor Dashboard:** Own appointment stats, approval rate, upcoming schedule.
- **Patient Dashboard:** Upcoming/completed/cancelled counts, pending approvals, recent activity.
- **Reports:** Appointment report (date-range filterable), doctor performance, monthly trend (1–24 months), full snapshot.

### 📤 Data Export (CSV / Excel / PDF)
- **CSV** — UTF-8 BOM encoded, formula-injection-safe (`=`, `+`, `-`, `@` prefixed with `'`).
- **Excel** — Styled `.xlsx` workbook via ClosedXML with colour-coded status cells and frozen header rows.
- **PDF** — Landscape A4 report via QuestPDF with pagination, header/footer, and alternating row colours.

### 🔔 Real-Time Notifications (SignalR)
- `NotificationHub` pushes instant alerts to connected clients when appointment status changes (approved, rejected, cancelled, completed).

### 📧 Email Notifications
- Automated emails sent via SMTP on: appointment request, approval, rejection, completion, and cancellation.

### 🔍 Search & Pagination
- `SearchService` provides cross-entity search for Patients and Doctors.
- `PaginationService` provides generic pagination for all list endpoints.

---

## 👥 User Roles & Permissions

| Feature | Admin | Doctor | Patient |
|---|:---:|:---:|:---:|
| Manage Departments (CRUD) | ✅ | ❌ | ❌ |
| Create / Edit Doctors | ✅ | ❌ | ❌ |
| Update Own Doctor Profile | ❌ | ✅ | ❌ |
| Manage Working Hours | ✅ | ✅ (own) | ❌ |
| View All Patients | ✅ | ✅ | ❌ |
| View System Reports & Stats | ✅ | ❌ | ❌ |
| View All Appointments | ✅ | ✅ (own) | ❌ |
| Book Appointments | ✅ | ✅ | ✅ |
| Approve / Reject Appointment | ❌ | ✅ (own) | ❌ |
| Complete Appointment | ❌ | ✅ (own) | ❌ |
| Mark No-Show | ✅ | ✅ | ❌ |
| Cancel Appointment | ✅ | ✅ | ✅ (own) |
| Reschedule Appointment | ✅ | ✅ | ✅ (own) |
| Create Medical Records | ❌ | ✅ | ❌ |
| View Medical Records | ✅ | ✅ (own) | ✅ (own) |
| View All Invoices | ✅ | ❌ | ❌ |
| Pay Invoice (Stripe) | ❌ | ❌ | ✅ (own) |
| Cancel Invoice | ✅ | ❌ | ❌ |
| Export CSV / Excel / PDF | ✅ | ✅ | ✅ |

---

## 🔄 Appointment Lifecycle

```
Patient Books
     │
     ▼
[Status: Scheduled | ApprovalStatus: Pending]
     │
     ├── Doctor Rejects ──────────────────────► [ApprovalStatus: Rejected] ──► END
     │
     └── Doctor Approves
              │
              ▼
      [ApprovalStatus: Approved]
              │
              ├── Patient / Doctor Cancels ────► [Status: Cancelled] ──────────► END
              │
              ├── Patient / Doctor Reschedules ► [New Appointment Created, IsRescheduled = true]
              │
              ├── Doctor marks No-Show ────────► [Status: NoShow] ─────────────► END
              │
              └── Doctor Completes
                       │
                       ▼
               [Status: Completed]
                       │
                       ▼
               Invoice Auto-Generated
                       │
                       ▼
               Patient Pays (Stripe) ──────────► [Invoice.IsPaid = true] ──────► END
```

Every transition is recorded in `AppointmentStatusHistory`.

---

## 💳 Billing & Stripe Workflow

1. **Invoice Created** — automatically when a Doctor marks an appointment as `Completed`.
2. **Patient Views Invoice** — from their dashboard or billing page.
3. **Payment Initiated** — Patient clicks "Pay"; the API calls `StripePaymentService.CreateCheckoutSessionAsync()`.
4. **Stripe Hosted Checkout** — Patient is redirected to Stripe's secure payment page.
5. **Webhook Reconciliation** — Stripe sends `checkout.session.completed` to `POST /api/billing/webhook`. The service validates the signature and marks the invoice `IsPaid = true`.

---

## 📡 API Endpoints Reference

### 🔐 Auth — `POST /api/auth`
| Endpoint | Auth | Description |
|---|---|---|
| `POST /api/auth/login` | Public | Login; returns JWT token |
| `POST /api/auth/register` | Public | Register new user |
| `POST /api/auth/validate` | Public | Validate existing JWT token |

### 📅 Appointments — `/api/appointments`
| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| GET | `/api/appointments` | Any | All appointments (role-filtered) |
| GET | `/api/appointments/{id}` | Any | Get by ID with ownership check |
| GET | `/api/appointments/patient/{patientId}` | Admin / Own Patient | Appointments for a patient |
| GET | `/api/appointments/doctor/{doctorId}` | Admin / Own Doctor | Appointments for a doctor |
| GET | `/api/appointments/date-range` | Any | Filter by date range |
| GET | `/api/appointments/status/{status}` | Any | Filter by status enum |
| GET | `/api/appointments/my` | Any | Current user's appointments |
| GET | `/api/appointments/upcoming` | Any | Upcoming appointments |
| GET | `/api/appointments/history` | Any | Past appointment history |
| POST | `/api/appointments` | Any | Create appointment |
| POST | `/api/appointments/check-conflict` | Any | Check for scheduling conflict |
| PUT | `/api/appointments/{id}` | Any | Update appointment |
| PATCH | `/api/appointments/{id}/status` | Admin, Doctor | Change status directly |
| DELETE | `/api/appointments/{id}` | Any | Cancel appointment |
| GET | `/api/appointments/export/csv` | Any | Export appointments to CSV |
| GET | `/api/appointments/export/excel` | Any | Export appointments to Excel |
| GET | `/api/appointments/export/pdf` | Any | Export appointments to PDF |

### 🔄 Appointment Workflow — `/api/appointmentworkflow`
| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| POST | `/api/appointmentworkflow/request` | Patient | Request a new appointment |
| POST | `/api/appointmentworkflow/{id}/approve` | Doctor | Approve a pending appointment |
| POST | `/api/appointmentworkflow/{id}/reject` | Doctor | Reject with reason |
| POST | `/api/appointmentworkflow/{id}/complete` | Doctor | Complete with diagnosis/prescription |
| POST | `/api/appointmentworkflow/{id}/cancel` | Any | Cancel |
| POST | `/api/appointmentworkflow/{id}/reschedule` | Any | Reschedule to new date/time |
| POST | `/api/appointmentworkflow/{id}/no-show` | Doctor, Admin | Mark patient as no-show |
| GET | `/api/appointmentworkflow/pending-approvals` | Doctor | List pending appointments |
| GET | `/api/appointmentworkflow/doctors/{id}/available-slots` | Public | Get available time slots |
| GET | `/api/appointmentworkflow/{id}/status-history` | Any | Full status change history |

### 👨‍⚕️ Doctors — `/api/doctors`
| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| GET | `/api/doctors` | Any | List all doctors (paginated) |
| GET | `/api/doctors/{id}` | Any | Get doctor by ID |
| GET | `/api/doctors/user/{userId}` | Any | Get doctor by user ID |
| GET | `/api/doctors/{id}/calendar` | Any | Monthly appointment calendar |
| GET | `/api/doctors/available` | Any | List currently available doctors |
| GET | `/api/doctors/specialization/{spec}` | Any | Filter by specialization |
| POST | `/api/doctors` | Admin | Create doctor |
| PUT | `/api/doctors/{id}` | Admin, Doctor | Update profile |
| DELETE | `/api/doctors/{id}` | Admin | Delete doctor |

### 👤 Patients — `/api/patients`
| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| GET | `/api/patients` | Admin, Doctor | List all patients (paginated) |
| GET | `/api/patients/{id}` | Any | Get by ID (ownership check) |
| GET | `/api/patients/user/{userId}` | Any | Get by user ID |
| POST | `/api/patients` | Public | Register patient |
| DELETE | `/api/patients/{id}` | Admin | Delete patient |

### 🏢 Departments — `/api/departments`
| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| GET | `/api/departments` | Public | List all departments |
| GET | `/api/departments/{id}` | Public | Get by ID |
| POST | `/api/departments` | Admin | Create department |
| PUT | `/api/departments/{id}` | Admin | Update department |
| DELETE | `/api/departments/{id}` | Admin | Delete department |

### 🗂️ Medical Records — `/api/medicalrecords`
| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| GET | `/api/medicalrecords/patient/{patientId}` | Any | Records for a patient |
| GET | `/api/medicalrecords/my-records` | Patient | Own medical records |
| GET | `/api/medicalrecords/doctor/{doctorId}` | Doctor, Admin | Records by doctor |
| GET | `/api/medicalrecords/my-created-records` | Doctor | Records created by self |
| GET | `/api/medicalrecords/{id}` | Any | Get by ID |
| POST | `/api/medicalrecords` | Doctor | Create medical record |
| DELETE | `/api/medicalrecords/{id}` | Doctor, Admin | Delete record |

### 💳 Billing — `/api/billing`
| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| GET | `/api/billing` | Admin | List all invoices |
| GET | `/api/billing/{id}` | Any | Get invoice (ownership check) |
| GET | `/api/billing/patient/{patientId}` | Any | Invoices by patient |
| GET | `/api/billing/my-invoices` | Patient | Own invoices |
| GET | `/api/billing/pending` | Patient | Pending (unpaid) invoices |
| POST | `/api/billing/{id}/pay` | Patient | Create Stripe Checkout session |
| POST | `/api/billing/webhook` | Public (Stripe) | Process Stripe webhook events |
| DELETE | `/api/billing/{id}` | Admin | Cancel invoice |

### 📊 Reports — `/api/reports`
| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| GET | `/api/reports/appointments` | Admin | Full appointment report (date filterable) |
| GET | `/api/reports/doctor/{doctorId}` | Doctor, Admin | Doctor performance report |
| GET | `/api/reports/my-performance` | Doctor | Own performance report |
| GET | `/api/reports/system-stats` | Admin | System-wide statistics |
| GET | `/api/reports/monthly-trend` | Admin | Monthly appointment trend |
| GET | `/api/reports/dashboard` | Any | Role-based dashboard data |
| GET | `/api/reports/dashboard/admin` | Admin | Admin dashboard |
| GET | `/api/reports/dashboard/doctor` | Doctor | Doctor dashboard |
| GET | `/api/reports/dashboard/patient` | Patient | Patient dashboard |
| GET | `/api/reports/cards` | Admin | Quick-stat cards |
| GET | `/api/reports/full` | Admin | Full snapshot report |

### ⏰ Working Hours — `/api/doctors/{doctorId}/working-hours`
| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| GET | `/api/doctors/{doctorId}/working-hours` | Public | Get schedule |
| PUT | `/api/doctors/{doctorId}/working-hours` | Doctor, Admin | Update schedule |
| GET | `/api/my-working-hours` | Doctor | Get own schedule |
| PUT | `/api/my-working-hours` | Doctor | Update own schedule |

---

## 🔐 Security Implementation

| Mechanism | Details |
|---|---|
| **JWT Authentication** | Issued on login; configurable expiry (default 1440 min = 24 h); secret loaded exclusively from environment variables / appsettings |
| **Cookie Authentication** | `HttpOnly = true`, `SecurePolicy = Always`, `SameSite = Strict` — prevents XSS cookie theft and CSRF |
| **Password Hashing** | BCrypt with automatic salt generation |
| **Rate Limiting** | Global: 100 req/min per user or IP. Auth endpoints: 5 req/min per IP |
| **Anti-Forgery (CSRF)** | Anti-forgery tokens on all Web MVC POST forms; `[ValidateAntiForgeryToken]` enforced |
| **Security Headers** | `SecurityHeadersMiddleware` adds `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy` |
| **FluentValidation** | Input validated at the controller boundary before any service code executes |
| **CORS** | Restricted to known trusted origins only |
| **Global Exception Handling** | `ExceptionMiddleware` (API) + `GlobalExceptionHandlerMiddleware` (Web) — prevents stack trace leakage |
| **Serilog Request Logging** | All HTTP requests logged with timing, status, and user for audit trail |
| **Optimistic Concurrency** | `[Timestamp] RowVersion` on `Appointment`, `Patient`, `Doctor` to prevent lost updates |
| **Health Check** | `/health` endpoint with SQL Server connectivity probe |

---

## ⚙️ Configuration Reference

| Configuration Key | Location | Description |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `appsettings.json` | SQL Server connection string |
| `ConnectionStrings:Redis` | `appsettings.json` (API) | Redis (optional — currently uses in-memory) |
| `JwtSettings:Secret` | `appsettings.json` | JWT signing secret (min 32 chars) |
| `JwtSettings:Issuer` | `appsettings.json` | Token issuer (`HospitalMS.API`) |
| `JwtSettings:Audience` | `appsettings.json` | Token audience (`HospitalMS.Web`) |
| `JwtSettings:ExpirationInMinutes` | `appsettings.json` | Token TTL (default: `1440`) |
| `AppointmentSettings:DefaultSlotDurationMinutes` | `appsettings.json` | Appointment slot length (default: `30`) |
| `AppointmentSettings:MaxAdvanceBookingDays` | `appsettings.json` | Max days ahead allowed for booking (default: `90`) |
| `AppointmentSettings:MinCancellationHours` | `appsettings.json` | Min hours notice for cancellation (default: `24`) |
| `AppointmentSettings:MaxAppointmentsPerDay` | `appsettings.json` | Max appointments a doctor can have per day (default: `20`) |
| `ApiSettings:BaseUrl` | `Web/appsettings.json` | API base URL for MVC ↔ API communication |
| `Stripe:PublishableKey` | `appsettings.json` | Stripe public key |
| `Stripe:SecretKey` | `appsettings.json` | Stripe secret key |
| `Stripe:WebhookSecret` | `appsettings.json` | Stripe webhook signing secret |
| `Email:SmtpServer` | `appsettings.json` | SMTP server host |
| `Email:SmtpPort` | `appsettings.json` | SMTP port (default: `587`) |
| `Email:SmtpUsername` | `appsettings.json` | SMTP credentials |
| `Serilog` | `appsettings.json` | Log level and sinks (Console + rolling file) |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (LocalDB, Express, or Full)
- Visual Studio 2022+ or VS Code
- *(Optional)* [Stripe Account](https://dashboard.stripe.com/) for payment features

### Step-by-Step Setup

**1. Clone the repository**
```bash
git clone https://github.com/Jeetvachhani123/HospitalManagementSystem.git
cd HospitalManagementSystem
```

**2. Configure database connection strings**

Open both files and update the `DefaultConnection`:
- `HospitalMS.API/appsettings.json`
- `HospitalMS.Web/appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=HospitalMS_DB;Trusted_Connection=True;TrustServerCertificate=True"
}
```

**3. Set your JWT Secret**

In `HospitalMS.API/appsettings.json` and `HospitalMS.Web/appsettings.json`:
```json
"JwtSettings": {
  "Secret": "YOUR_STRONG_SECRET_KEY_MINIMUM_32_CHARACTERS"
}
```

**4. Apply database migrations**
```bash
cd HospitalMS.API
dotnet ef database update --project ../HospitalMS.DATA
```

**5. Run both projects simultaneously**

**Option A — CLI (two terminals)**
```bash
# Terminal 1
cd HospitalMS.API
dotnet run

# Terminal 2
cd HospitalMS.Web
dotnet run
```

**Option B — Visual Studio**
Right-click the Solution → Properties → **Multiple Startup Projects** → set both `HospitalMS.API` and `HospitalMS.Web` to **Start**.

**6. Access the application**

| Service | URL |
|---|---|
| REST API (Swagger UI) | `https://localhost:7058/swagger` |
| Health Check | `https://localhost:7058/health` |
| MVC Web Application | `https://localhost:7066` |

---

## 👨‍💻 Author

**Jeet Vachhani**
- 🎓 BCA Graduate
- 💼 .NET Developer
- 🛠️ Skills: C#, ASP.NET Core, Entity Framework Core, SQL Server, Clean Architecture, MVC, SignalR, Stripe, REST APIs

---

<div align="center">

⭐ **If you found this project useful, please give it a star!** ⭐

</div>
