# MedCenter Clinic Management System

A full-stack healthcare clinic management system built with **ASP.NET Core (.NET 9)**, **Entity Framework Core**, **SQL Server**, and **ASP.NET Core Identity**. Developed as a group project for the IT8118 module.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Team & Stage Responsibilities](#team--stage-responsibilities)
- [Prerequisites](#prerequisites)
- [Setup & Run on Windows 10 / 11](#setup--run-on-windows-10--11)
- [Seed Accounts](#seed-accounts)
- [Project Structure](#project-structure)

---

## Project Overview

MedCenter is a multi-role clinic management system that covers the full patient journey — from booking an appointment through to receiving a diagnosis, prescription, and medical history record.

**Key capabilities:**

- Role-based access for **Patients**, **Doctors**, **Receptionists**, and **Clinic Managers**
- Multi-step appointment booking with real-time doctor availability calculation
- Full appointment lifecycle management (Requested → Confirmed → Checked-In → In-Progress → Completed / Cancelled / Missed)
- Doctor schedule and leave management
- Visit records and prescription creation and printing
- In-system notification bell with auto-refresh
- Web API with JWT authentication for external integrations
- Separate reporting application for clinic managers (HttpClient-based, no direct DB access)
- Real-time appointment tracking via SignalR
- Public appointment lookup page

---

## Architecture

The solution (`ClinicSystem.sln`) contains three projects:

| Project | Type | Role |
|---|---|---|
| `ClinicAPI` | ASP.NET Core Web API | Database layer, REST API, JWT auth, SignalR hub |
| `ClinicMVC` | ASP.NET Core MVC | Main patient/staff web application (cookie auth, EF Core direct) |
| `ClinicReporting` | ASP.NET Core MVC | Manager reporting app (HttpClient only, no DB reference) |

```
ClinicSystem/
├── ClinicAPI/          ← Web API + EF Core models + migrations
├── ClinicMVC/          ← Main MVC app (Identity, booking, lifecycle, notifications)
└── ClinicReporting/    ← Reporting app (JWT session, HttpClient calls to API)
```

---

## Technology Stack

- **Framework:** ASP.NET Core 9 (MVC + Web API)
- **ORM:** Entity Framework Core 9 with SQL Server
- **Authentication:** ASP.NET Core Identity (MVC cookie) + JWT Bearer (API / Reporting)
- **Real-time:** SignalR
- **Frontend:** Bootstrap 5.3, Font Awesome 6.5, AOS animations, Particles.js
- **Database:** SQL Server Express (local development)
- **IDE:** Visual Studio 2022

---

## Team & Stage Responsibilities

| Member | Stages | Areas |
|---|---|---|
| **Ali Abdullah** | Stage 1, 2, 9 | Environment & repo setup, database design & entity layer, Web API endpoints |
| **Ali Alsaffar** | Stage 3, 5, 6 | Authentication & Identity, appointment booking wizard, appointment lifecycle management |
| **Faisal Alasfoor** | Stage 4, 7, 8 | Doctor & schedule management, visit records & prescriptions, in-system notifications |
| **Abdalrahman** | Stage 10–15 | Public lookup page, SignalR real-time tracking, reporting application, UI polish, Azure deployment |

### Stage Breakdown

| Stage | Title | Owner |
|---|---|---|
| 1 | Environment & Repository Setup | Ali Abdullah |
| 2 | Database Design & Entity Layer | Ali Abdullah |
| 3 | Authentication & Identity | Ali Alsaffar |
| 4 | Doctor & Schedule Management | Faisal Alasfoor |
| 5 | Appointment Booking | Ali Alsaffar |
| 6 | Appointment Lifecycle Management | Ali Alsaffar |
| 7 | Visit Records & Prescriptions | Faisal Alasfoor |
| 8 | In-System Notifications | Faisal Alasfoor |
| 9 | Web API Endpoints | Ali Abdullah |
| 10 | Public Appointment Lookup Page | Abdalrahman |
| 11 | SignalR Real-Time Tracking | Abdalrahman |
| 12 | Reporting Application | Abdalrahman |
| 13 | UI Polish & Quality Pass | Abdalrahman |
| 14 | Enhancements (Bonus) | Abdalrahman |
| 15 | Azure Deployment | Abdalrahman |

---

## Prerequisites

Install the following before setting up the project:

1. **Visual Studio 2022 Community** (free) — [visualstudio.microsoft.com](https://visualstudio.microsoft.com/)
   - During install select workload: **ASP.NET and web development**
   - Also tick: **.NET desktop development**
2. **SQL Server Express** (free) — [microsoft.com/sql-server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
3. **SQL Server Management Studio (SSMS)** (optional, for inspecting the database) — [aka.ms/ssmsfullsetup](https://aka.ms/ssmsfullsetup)
4. **Git** — [git-scm.com](https://git-scm.com/)
5. **.NET 9 SDK** — included with Visual Studio 2022 v17.8+, or download from [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

---

## Setup & Run on Windows 10 / 11

### Step 1 — Clone the repository

Open **Command Prompt** or **Git Bash** and run:

```bash
git clone https://github.com/VolcanoPoly/clinic-management-system.git
cd clinic-management-system
```

### Step 2 — Open the solution in Visual Studio

1. Open **Visual Studio 2022**
2. Click **Open a project or solution**
3. Navigate to `clinic-management-system/ClinicSystem/` and open **`ClinicSystem.sln`**

### Step 3 — Configure the database connection string

1. In **Solution Explorer**, open `ClinicAPI/appsettings.json`
2. Update the `DefaultConnection` to match your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=ClinicDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

> If your SQL Server instance name is different (e.g., `localhost` or `.\MSSQLSERVER`), update `Server=` accordingly. You can find your instance name in SSMS when you connect.

3. Repeat the same connection string update in `ClinicMVC/appsettings.json`.

### Step 4 — Apply database migrations

1. In Visual Studio, open the **Package Manager Console**:
   `Tools → NuGet Package Manager → Package Manager Console`
2. Run:

```powershell
Update-Database -Project ClinicAPI -StartupProject ClinicAPI
```

This creates the `ClinicDB` database and all tables. Seed data (specializations, doctors, patients, and sample appointments) is applied automatically the first time the API starts.

### Step 5 — Configure multiple startup projects

1. Right-click the **solution** in Solution Explorer → **Set Startup Projects...**
2. Select **Multiple startup projects**
3. Set the action to **Start** for all three projects:
   - `ClinicAPI`
   - `ClinicMVC`
   - `ClinicReporting`
4. Click **OK**

### Step 6 — Run the application

Press **F5** (or click the green **Start** button).

Three browser tabs will open automatically:

| Application | URL |
|---|---|
| ClinicAPI (Swagger) | `https://localhost:7053/swagger` |
| ClinicMVC (Main App) | `https://localhost:7268` |
| ClinicReporting | `https://localhost:7298` |

> If you see a certificate warning in the browser, click **Advanced → Proceed to localhost**. This is expected for local HTTPS in development.

---

## Seed Accounts

The database is pre-seeded with the following test accounts:

### Clinic Manager
- **Email:** `manager@medcenter.com`
- **Password:** `Manager@123`

### Receptionist
- **Email:** `receptionist@medcenter.com`
- **Password:** `Recept@123`

### Doctors

| Name | Email | Password | Specializations |
|---|---|---|---|
| Dr. Omar Hassan | `doctor1@medcenter.com` | `Doctor@123` | General Practice, Cardiology |
| Dr. Fatima Al-Zahra | `doctor2@medcenter.com` | `Doctor@123` | General Practice, Pediatrics |
| Dr. Ahmed Al-Nouri | `doctor3@medcenter.com` | `Doctor@123` | Neurology, General Practice |
| Dr. Sara Khalifa | `doctor4@medcenter.com` | `Doctor@123` | Ophthalmology, ENT |
| Dr. Tariq Al-Sayed | `doctor5@medcenter.com` | `Doctor@123` | Psychiatry |
| Dr. Maryam Jaffar | `doctor6@medcenter.com` | `Doctor@123` | Gynecology, Dermatology |

### Patients

| Name | Email | Password | CPR | Reference |
|---|---|---|---|---|
| Yousef Mansoor | `patient1@medcenter.com` | `Patient@123` | `860101001` | `PAT-0001` |
| Layla Qassim | `patient2@medcenter.com` | `Patient@123` | `920515002` | `PAT-0002` |

You can also **register a new patient account** directly at `/Account/Register`.

---

## Azure Deployment Settings

Create three Azure App Services: one each for `ClinicAPI`, `ClinicMVC`, and `ClinicReporting`. Do not put production secrets in `appsettings.json`; use App Service Configuration settings.

### ClinicAPI App Settings

| Name | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | Azure SQL connection string |
| `JwtSettings__SecretKey` | A strong secret, at least 32 characters |
| `JwtSettings__Issuer` | `ClinicAPI` |
| `JwtSettings__Audience` | `ClinicClients` |
| `AllowedOrigins__MvcApp` | Deployed MVC URL, e.g. `https://your-clinic-mvc.azurewebsites.net` |
| `AllowedOrigins__ReportingApp` | Deployed Reporting URL, e.g. `https://your-clinic-reporting.azurewebsites.net` |

### ClinicMVC App Settings

| Name | Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | Same Azure SQL connection string |
| `ApiBaseUrl` | Deployed API URL, e.g. `https://your-clinic-api.azurewebsites.net/` |

### ClinicReporting App Settings

| Name | Value |
|---|---|
| `ApiBaseUrl` | Deployed API URL, e.g. `https://your-clinic-api.azurewebsites.net/` |

After deployment, enable WebSockets on the MVC App Service if the SignalR waiting room/status board does not update live.

---

## Project Structure

```
ClinicSystem/
│
├── ClinicAPI/
│   ├── Controllers/        ← REST API controllers (JWT-protected)
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── DataSeeder.cs   ← Seed data applied on startup
│   ├── Hubs/               ← SignalR hub for real-time updates
│   ├── Models/             ← Entity models (Doctor, Patient, Appointment, etc.)
│   └── Migrations/         ← EF Core migration history
│
├── ClinicMVC/
│   ├── Controllers/
│   │   ├── AccountController.cs       ← Stage 3  (Ali Alsaffar)
│   │   ├── AppointmentController.cs   ← Stage 5 & 6  (Ali Alsaffar)
│   │   ├── DoctorController.cs        ← Stage 7  (Faisal Alasfoor)
│   │   ├── ManagerController.cs       ← Stage 4  (Faisal Alasfoor)
│   │   ├── NotificationController.cs  ← Stage 8  (Faisal Alasfoor)
│   │   └── PatientController.cs       ← Stage 7  (Faisal Alasfoor)
│   ├── Models/ViewModels/  ← View models for each feature area
│   ├── Services/           ← AvailabilityService, NotificationService
│   ├── Views/              ← Razor views organised by controller
│   └── wwwroot/            ← Static assets (site.css, logo.svg, JS)
│
└── ClinicReporting/
    ├── Controllers/        ← Reporting controllers (JWT session-based)
    ├── Views/              ← Report views
    └── wwwroot/            ← Shared static assets
```

---

> Built with ASP.NET Core 9 · Entity Framework Core · SQL Server · Bootstrap 5.3
