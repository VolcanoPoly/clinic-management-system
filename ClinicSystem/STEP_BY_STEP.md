# IT8118 — Complete Sequential Step-by-Step Guide
## Healthcare Clinic System (Brief A) — Ali + AI Agent Workflow

> Follow every step in order. Do not skip steps. Each step says exactly who does it and what to do.  
> **Ali** = you do it manually. **AI Agent** = ask the AI Code Agent to do it (paste the prompt given).  
> Check the box when a step is done before moving to the next.

---

## STAGE 1 — Environment & Repository Setup (Member 1)

---

### Step 1 — `ALI`
**Install required software on your machine**

- [x] Download and install **Visual Studio 2022 Community** (free)
  - During install, select workload: **"ASP.NET and web development"**
  - Also tick: **.NET desktop development**
- [x] Download and install **SQL Server Express** (free) — for local database
- [x] Download and install **SQL Server Management Studio (SSMS)** or **Azure Data Studio** — to view your database
- [x] Download and install **Git** (git-scm.com) if not already installed
- [x] Make sure you have a **GitHub account** (github.com) — your commits must come from your account

---

### Step 2 — `ALI`
**Create the GitHub repository**

- [x] Go to github.com and log in with your account
- [x] Click "New Repository"
- [x] Name it: `ClinicSystem-IT8118`
- [x] Set it to **Public** (required by the assessment)
- [x] Tick "Add a README file"
- [x] Click "Create repository"
- [x] Copy the repository URL (you'll need it shortly)
- [x] Open a terminal/command prompt and run:
  ```bash
  git clone <your-repo-url>
  cd ClinicSystem-IT8118
  ```

---

### Step 3 — `ALI`
**Open Visual Studio and create the solution**

- [x] Open Visual Studio 2022
- [x] Click "Create a new project"
- [x] Search for **"Blank Solution"** → select it → click Next
- [x] Name the solution: `ClinicSystem`
- [x] Set the location to your cloned repo folder (`ClinicSystem-IT8118`)
- [x] Click Create
- [x] You now have an empty solution — leave Visual Studio open

---

### Step 4 — `AI AGENT`
**Prompt to give the AI:**

> "Create the three ASP.NET Core projects inside a Visual Studio solution for Brief A (Healthcare Clinic system). The solution is named `ClinicSystem`. Create:
> 1. `ClinicAPI` — ASP.NET Core Web API (.NET 9)
> 2. `ClinicMVC` — ASP.NET Core MVC (.NET 9), with a project reference to ClinicAPI
> 3. `ClinicReporting` — ASP.NET Core MVC (.NET 9), NO project reference to ClinicAPI
>
> For each project, give me the exact steps to add it in Visual Studio and the complete `Program.cs` starter code with all needed services registered (EF Core, Identity, JWT, SignalR for ClinicAPI; EF Core, Identity, HttpClient for ClinicMVC; HttpClient only for ClinicReporting). Also give me the `appsettings.json` for all three with placeholder connection string and JWT settings."

- [x] The AI will give you code and steps — follow them to add all 3 projects to your solution

---

### Step 5 — `AI AGENT` ✅ COMPLETED
**Prompt to give the AI:**

> "Give me the complete list of NuGet packages I need to install for each of the three projects in the ClinicSystem solution. For each package, tell me which project it goes in, the package name, and the exact command to install it via the NuGet Package Manager Console."

- [x] Open **Tools → NuGet Package Manager → Package Manager Console** in Visual Studio
- [x] Run every command the AI gives you
- [x] Confirm all packages install without errors

> Completed as part of Step 4 — NuGet install commands were included in the Step 4 GUIDE.md.

---

### Step 6 — `ALI` ✅ COMPLETED
**First commit — push the empty solution to GitHub**

- [x] In your terminal (inside the repo folder), run:
  ```bash
  git add .
  git commit -m "Initial solution setup with 3 projects"
  git push origin main
  ```
- [x] Go to github.com and confirm your repo shows the files

> Completed as part of Step 4 — commit and push were included in the Step 4 GUIDE.md and done by the team.

---

## STAGE 2 — Database Design & Entity Layer (Member 1)

---

### Step 7 — `AI AGENT`
**Prompt to give the AI:**

> "Design and write all entity/domain classes for the Healthcare Clinic system (Brief A) in the `ClinicAPI/Models/` folder. I need the following entities:
> - `ApplicationUser` (extends IdentityUser, adds FirstName, LastName)
> - `Doctor` (linked to ApplicationUser, has LicenseNumber, Bio)
> - `Specialization` (Id, Name, Description)
> - `DoctorSpecialization` (junction table — DoctorId, SpecializationId)
> - `DoctorSchedule` (DoctorId, DayOfWeek, StartTime, EndTime)
> - `DoctorLeave` (DoctorId, StartDate, EndDate, Reason)
> - `Patient` (linked to ApplicationUser, CPRNumber, ReferenceNumber, DateOfBirth, BloodType, EmergencyContact)
> - `Appointment` (PatientId, DoctorId, SpecializationId, AppointmentDateTime, Status enum, Notes, CancellationReason)
> - `AppointmentStatusHistory` (AppointmentId, OldStatus, NewStatus, ChangedAt, ChangedByUserId)
> - `VisitRecord` (AppointmentId, DoctorNotes, Diagnosis, Treatment, CreatedAt)
> - `Prescription` (VisitRecordId, DoctorId, IssuedAt)
> - `PrescriptionItem` (PrescriptionId, MedicationName, Dosage, Frequency, Duration, Instructions)
> - `Notification` (RecipientUserId, Message, IsRead, CreatedAt, RelatedAppointmentId)
>
> Also write the `AppointmentStatus` enum with values: Requested, Confirmed, CheckedIn, InProgress, Completed, Cancelled, Missed.
> Write each class as a complete C# file."

- [x] Create a `Models` folder inside `ClinicAPI`
- [x] Create each file the AI gives you inside that folder

---

### Step 8 — `AI AGENT`
**Prompt to give the AI:**

> "Write the complete `ApplicationDbContext.cs` for the ClinicSystem project. It should:
> - Extend `IdentityDbContext<ApplicationUser>`
> - Include DbSet for every entity (Doctor, Patient, Appointment, VisitRecord, Prescription, PrescriptionItem, Notification, DoctorSchedule, DoctorLeave, Specialization, DoctorSpecialization, AppointmentStatusHistory)
> - Use Fluent API in `OnModelCreating` to configure all relationships (one-to-many, many-to-many with DoctorSpecialization junction table, cascade delete rules)
> - Seed the 4 roles: Patient, Doctor, Receptionist, ClinicManager
> Place this in `ClinicAPI/Data/ApplicationDbContext.cs`"

- [x] Create a `Data` folder inside `ClinicAPI`
- [x] Create `ApplicationDbContext.cs` with the AI's code

---

### Step 9 — `ALI`
**Update connection string and register DbContext**

- [x] Open `ClinicAPI/appsettings.json`
- [x] Update the connection string to point to your local SQL Server:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ClinicSystemDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
  ```
- [x] Open `ClinicMVC/appsettings.json` and add the same connection string
- [x] Confirm `Program.cs` in both `ClinicAPI` and `ClinicMVC` registers the DbContext (the AI should have done this in Step 4 — verify it is there)

---

### Step 10 — `ALI`
**Create and run the first EF Core migration**

- [x] Open **Tools → NuGet Package Manager → Package Manager Console**
- [x] Set the "Default Project" dropdown to `ClinicAPI`
- [x] Run:
  ```
  Add-Migration InitialCreate -StartupProject ClinicAPI
  ```
- [x] Wait for it to complete — a `Migrations` folder will appear in `ClinicAPI`
- [x] Then run:
  ```
  Update-Database -StartupProject ClinicAPI
  ```
- [x] Open SSMS or Azure Data Studio → connect to `(localdb)\mssqllocaldb` → confirm database `ClinicSystemDB` was created with all tables

---

### Step 11 — `AI AGENT`
**Prompt to give the AI:**

> "Write a complete SQL seed script (`seed.sql`) for the ClinicSystem database. It should insert:
> - 1 Clinic Manager user account (email: manager@clinic.com, password hash for: Manager@123)
> - 2 Doctor user accounts (doctor1@clinic.com / Doctor@123, doctor2@clinic.com / Doctor@123)
> - 1 Receptionist user account (receptionist@clinic.com / Recept@123)
> - 2 Patient user accounts (patient1@clinic.com / Patient@123, patient2@clinic.com / Patient@123)
> - Specializations: General Practice, Cardiology, Dermatology, Pediatrics
> - Doctor profiles linked to the doctor users, each with 2 specializations
> - Doctor schedules (Mon–Fri, 8am–5pm)
> - Patient profiles with CPR numbers and reference numbers
> - 5 sample appointments in various statuses
> - 2 VisitRecords with prescriptions for completed appointments
> - Sample notifications
>
> Use ASP.NET Core Identity password hashing format. Assign correct roles to each user in the AspNetUserRoles table."

- [x] Save the file as `seed.sql` in the root of your repo
- [x] Open SSMS → connect to your local DB → open the script → run it
- [x] Confirm data appears in the tables

---

### Step 12 — `ALI`
**Commit the database layer**

```bash
git add .
git commit -m "Add entity models, DbContext, initial migration, and seed data"
git push origin main
```

---

## STAGE 3 — Authentication & Identity (Member 2)

---

### Step 13 — `AI AGENT`
**Prompt to give the AI:**

> "Implement ASP.NET Core Identity authentication for the `ClinicMVC` project. I need:
> 1. A complete `AccountController.cs` with actions: Register (GET+POST), Login (GET+POST), Logout, AccessDenied
> 2. Razor views for: Register, Login (in `Views/Account/`)
> 3. A `RegisterViewModel.cs` and `LoginViewModel.cs` with validation attributes
> 4. The Register form should capture: FirstName, LastName, Email, Password, ConfirmPassword, and Role (dropdown: Patient only — public registration is for patients only)
> 5. After registration, assign the Patient role and create a `Patient` profile record
> 6. After login, redirect to a role-specific dashboard
> 7. A `_LoginPartial.cshtml` partial view showing login/logout links and the logged-in user's name + role
> 
> Use ASP.NET Core Identity (cookie-based) for the MVC app. Place all files in the correct folders."

- [x] Create all the files the AI gives you in the correct locations
- [x] Run the app (`F5` in Visual Studio) and test:
  - [x] Register a new patient account
  - [x] Log in with that account
  - [x] Log out

---

### Step 14 — `AI AGENT`
**Prompt to give the AI:**

> "Implement role-based dashboards in `ClinicMVC`. Create a `HomeController.cs` with an `Index` action that redirects to the correct dashboard based on the logged-in user's role:
> - Patient → `PatientController/Dashboard`
> - Doctor → `DoctorController/Dashboard`
> - Receptionist → `ReceptionistController/Dashboard`
> - ClinicManager → `ManagerController/Dashboard`
>
> Create stub controllers and views for each role with a simple welcome message and a placeholder navigation menu. Add `[Authorize(Roles = '...')]` to each controller. Also update `_Layout.cshtml` to show role-based navigation links."

- [x] Create all files
- [x] Test: log in as each seeded role and confirm you land on the correct dashboard
- [x] Confirm unauthorized access redirects to the login page

---

### Step 15 — `ALI`
**Commit authentication work**

```bash
git add .
git commit -m "Implement Identity authentication, role-based login, and dashboard routing"
git push origin main
```

---

## STAGE 4 — Core Feature: Doctor & Schedule Management (Member 3)

---

### Step 16 — `AI AGENT`
**Prompt to give the AI:**

> "Implement Doctor and Specialization management for the `ClinicManager` role in `ClinicMVC`. I need:
>
> 1. `ManagerController.cs` with actions for:
>    - List all doctors (with their specializations)
>    - Create doctor (creates both an ApplicationUser + Doctor record, assigns Doctor role, sets password)
>    - Edit doctor profile (name, bio, license number, specializations)
>    - View doctor schedule (their weekly working hours)
>    - Edit doctor schedule (add/edit/remove working days and hours)
>    - Add doctor leave (date range + reason)
>    - View upcoming leaves for a doctor
>    - Deactivate a doctor account
>
> 2. All corresponding Razor views (strongly-typed with ViewModels)
> 3. ViewModels: `DoctorListViewModel`, `DoctorCreateViewModel`, `DoctorEditViewModel`, `ScheduleViewModel`, `LeaveViewModel`
> 4. A `SpecializationController.cs` for CRUD of specializations (Clinic Manager only)
>
> Use EF Core directly (not API calls). Apply `[Authorize(Roles = 'ClinicManager')]` to all actions. Add success/error TempData messages."

- [ ] Create all files
- [ ] Log in as Clinic Manager and test:
  - [ ] Create a new doctor
  - [ ] Edit the doctor's specializations
  - [ ] Set a weekly schedule
  - [ ] Add a leave period
  - [ ] Add a new specialization

---

### Step 17 — `ALI`
**Commit doctor management features**

```bash
git add .
git commit -m "Add Clinic Manager: doctor profile, schedule, leave, and specialization management"
git push origin main
```

---

## STAGE 5 — Core Feature: Appointment Booking (Member 2)

---

### Step 18 — `AI AGENT`
**Prompt to give the AI:**

> "Implement the appointment booking feature in `ClinicMVC`. I need a multi-step booking flow:
>
> Step 1: Patient selects a specialization (dropdown of all specializations)
> Step 2: System shows doctors who have that specialization (with their next available dates)
> Step 3: Patient selects a doctor and a date — system shows available time slots for that date (respects the doctor's DoctorSchedule and DoctorLeave and existing Appointments — no double booking)
> Step 4: Patient confirms the booking — Appointment created with Status = Requested
>
> Requirements:
> - `AppointmentController.cs` with actions: `Book` (GET shows step 1), `SelectDoctor` (POST step 1 → GET step 2), `SelectSlot` (POST step 2 → GET step 3), `Confirm` (POST step 3 → creates appointment)
> - A `AvailabilityService.cs` that calculates available 30-minute time slots for a given doctor and date (filters out booked slots, leave periods, outside working hours)
> - Receptionist can also book — they see an extra dropdown to select which patient they are booking for
> - Strongly-typed ViewModels for each step
> - Validation: cannot book in the past, cannot book on a day the doctor is not available
> - On successful booking: create a Notification for the doctor ('New appointment request from [patient name]')
>
> Apply `[Authorize(Roles = 'Patient,Receptionist')]` to the booking actions."

- [ ] Create all files
- [ ] Test the full booking flow as a Patient
- [ ] Test booking as a Receptionist (selecting a patient)
- [ ] Try to book a slot that already exists — confirm it is blocked

---

### Step 19 — `ALI`
**Commit booking feature**

```bash
git add .
git commit -m "Implement multi-step appointment booking with availability checking and double-booking prevention"
git push origin main
```

---

## STAGE 6 — Core Feature: Appointment Lifecycle Management (Member 2)

---

### Step 20 — `AI AGENT`
**Prompt to give the AI:**

> "Implement appointment lifecycle management in `ClinicMVC`. The status flow is:
> Requested → Confirmed → CheckedIn → InProgress → Completed / Cancelled / Missed
>
> I need:
> 1. `AppointmentController.cs` additional actions:
>    - `MyAppointments` — Patient sees their own appointments
>    - `TodaysAppointments` — Receptionist sees today's appointments (sortable list)
>    - `DoctorAppointments` — Doctor sees their upcoming and past appointments
>    - `AllAppointments` — Clinic Manager sees all appointments with filters (date range, doctor, status)
>    - `UpdateStatus(int id, AppointmentStatus newStatus, string reason)` — POST action that:
>      a. Validates the transition is allowed (enforce the valid transitions only)
>      b. Updates the appointment status
>      c. Creates an `AppointmentStatusHistory` record
>      d. Sends a Notification to the relevant parties
>      e. If status = Completed: redirects to CreateVisitRecord page
>      f. If status = Missed/Cancelled: records reason
>    - `Cancel(int id)` — Patient can cancel their own upcoming appointment (only if Requested or Confirmed)
>
> 2. Role-based status buttons on the appointment detail view:
>    - Receptionist buttons: Confirm, Mark CheckedIn, Cancel
>    - Doctor buttons: Start (InProgress), Complete, Mark Missed
>    - Patient buttons: Cancel (only if Requested or Confirmed)
>
> 3. An `AppointmentDetailViewModel` showing full appointment info + status history timeline
> 4. All views strongly-typed, with Bootstrap badge colors per status
>
> Use EF Core directly. Notifications sent on every status change."

- [ ] Create all files
- [ ] Test the full lifecycle as each role:
  - [ ] Patient books → Receptionist confirms → Receptionist checks in → Doctor starts → Doctor completes
  - [ ] Try an invalid transition (e.g., Requested → Completed directly) — confirm it is blocked
  - [ ] Patient cancels a Requested appointment — confirm it works
  - [ ] Try cancelling a Completed appointment — confirm it is blocked

---

### Step 21 — `ALI`
**Commit lifecycle management**

```bash
git add .
git commit -m "Implement appointment lifecycle with status transitions, validation, and status history"
git push origin main
```

---

## STAGE 7 — Core Feature: Visit Records & Prescriptions (Member 3)

---

### Step 22 — `AI AGENT`
**Prompt to give the AI:**

> "Implement Visit Records and Prescriptions in `ClinicMVC`. I need:
>
> 1. After an appointment is marked Completed, the Doctor is redirected to a `CreateVisitRecord` page
> 2. `VisitRecordController.cs` with:
>    - `Create(int appointmentId)` GET+POST — Doctor fills in: Notes, Diagnosis, Treatment, then can add prescription items
>    - `Details(int id)` — view a visit record (Doctor sees all their patients'; Patient sees only their own)
>    - `PatientHistory(int patientId)` — full visit history for a patient (Doctor and Manager see this)
>    - `MyHistory()` — Patient sees their own full visit history
>
> 3. `PrescriptionController.cs` with:
>    - `Create(int visitRecordId)` GET+POST — Doctor adds a prescription with 1+ medication items
>    - `AddItem` — AJAX or partial to add another medication row dynamically
>    - `Details(int id)` — view a prescription (Patient sees their own; Doctor sees ones they wrote)
>    - `Print(int id)` — printer-friendly prescription view
>
> 4. ViewModels: `VisitRecordCreateViewModel`, `VisitRecordDetailsViewModel`, `PrescriptionViewModel`, `PrescriptionItemViewModel`
>
> 5. Access rules:
>    - Only the treating Doctor can create a VisitRecord for an appointment
>    - Patients can only view their own records
>    - Doctors can view records for appointments they conducted
>    - Clinic Manager can view all records
>
> Use EF Core directly."

- [ ] Create all files
- [ ] Test: complete an appointment → fill visit record → add prescription
- [ ] Log in as the patient → view visit history and prescription
- [ ] Confirm doctor can only see their own patients' records

---

### Step 23 — `ALI`
**Commit visit records and prescriptions**

```bash
git add .
git commit -m "Implement visit records, patient history, and prescription management"
git push origin main
```

---

## STAGE 8 — Core Feature: In-System Notifications (Member 2)

---

### Step 24 — `AI AGENT`
**Prompt to give the AI:**

> "Implement the in-system notifications feature in `ClinicMVC`. I need:
>
> 1. A `NotificationService.cs` with a method `SendNotification(string userId, string message, int? relatedAppointmentId)` — creates a Notification record in the DB. Call this from AppointmentController on every status change.
>
> 2. A `NotificationController.cs` with:
>    - `Index()` — list all notifications for the logged-in user, newest first, with unread highlighted
>    - `MarkRead(int id)` — mark one notification as read
>    - `MarkAllRead()` — mark all as read
>    - `GetUnreadCount()` — returns JSON count (for the bell badge)
>
> 3. Update `_Layout.cshtml` to show a notification bell icon in the navbar with:
>    - A red badge showing the unread count (fetched via AJAX every 30 seconds using `GetUnreadCount`)
>    - Clicking the bell links to the notification list page
>
> 4. Notifications should be sent when:
>    - New appointment booked → notify the Doctor
>    - Appointment Confirmed → notify the Patient
>    - Appointment Cancelled → notify the other party (if doctor cancels, notify patient; if patient cancels, notify doctor)
>    - Appointment Missed → notify the Patient
>    - Appointment Completed → notify the Patient ('Your visit record is ready')
>    - New prescription added → notify the Patient
>    - Doctor schedule changed and it affects an existing appointment → notify the Patient
>
> Use EF Core directly."

- [ ] Create all files
- [ ] Test: book appointment → confirm doctor gets notification
- [ ] Change status → confirm the right person gets a notification
- [ ] Check the bell badge updates without page refresh

---

### Step 25 — `ALI`
**Commit notifications**

```bash
git add .
git commit -m "Implement in-system notification system with bell badge and auto-refresh"
git push origin main
```

---

## STAGE 9 — Web API Endpoints (Member 1)

---

### Step 26 — `AI AGENT`
**Prompt to give the AI:**

> "Implement the Web API endpoints in `ClinicAPI`. I need these controllers:
>
> 1. `AuthController.cs`:
>    - `POST /api/auth/login` — accepts email+password, validates against Identity, returns a signed JWT token with claims (userId, email, role). No auth required.
>
> 2. `AppointmentsController.cs` (API):
>    - `GET /api/appointments/lookup?cpr={cpr}&ref={ref}` — PUBLIC (no JWT). Finds the patient by CPR+reference number, returns their upcoming appointments and last 3 visit summaries as JSON.
>    - `GET /api/appointments` — JWT required (Receptionist or ClinicManager). Returns list of appointments with filters (date, doctorId, status) as query params.
>    - `GET /api/appointments/{id}` — JWT required. Returns single appointment detail with status history.
>
> 3. `DoctorsController.cs` (API):
>    - `GET /api/doctors` — JWT required. Returns list of doctors with their specializations.
>    - `GET /api/doctors/{id}/availability?date={date}` — JWT required. Returns available 30-min time slots for the doctor on that date.
>
> 4. `ReportsController.cs` (API):
>    - `GET /api/reports/appointment-stats?from={date}&to={date}` — JWT required, ClinicManager role only. Returns: total appointments, by status breakdown, by specialization breakdown.
>    - `GET /api/reports/doctor-utilization?from={date}&to={date}` — JWT required, ClinicManager role only. Returns: appointments per doctor, completion rate per doctor.
>    - `GET /api/reports/cancellation-rates?from={date}&to={date}` — JWT required, ClinicManager role only. Returns: cancellation count, missed count, rates over time.
>
> Requirements:
> - All endpoints return proper HTTP status codes (200, 400, 401, 403, 404)
> - All data returned as JSON using response DTOs (not entity classes directly)
> - Write all DTO classes in `ClinicAPI/DTOs/` folder
> - Configure Swagger/OpenAPI so all endpoints are visible at `/swagger`
> - JWT configuration reads from appsettings.json"

- [ ] Create all controller and DTO files
- [ ] Run `ClinicAPI` project (`F5`)
- [ ] Open `https://localhost:{port}/swagger` in browser
- [ ] Test each endpoint in Swagger:
  - [ ] `POST /api/auth/login` with manager@clinic.com credentials → get a token
  - [ ] Click "Authorize" in Swagger → paste the token
  - [ ] Test `GET /api/appointments` → see list
  - [ ] Test `GET /api/appointments/lookup?cpr=...&ref=...` WITHOUT token → should work
  - [ ] Test `GET /api/reports/appointment-stats` WITHOUT token → should get 401

---

### Step 27 — `ALI`
**Commit API endpoints**

```bash
git add .
git commit -m "Implement Web API: auth (JWT), appointment lookup, doctor availability, and reporting endpoints"
git push origin main
```

---

## STAGE 10 — Public Appointment Lookup Page (HttpClient) (Member 4)

---

### Step 28 — `AI AGENT`
**Prompt to give the AI:**

> "Implement the public appointment lookup page in `ClinicMVC`. Requirements:
>
> 1. `LookupController.cs`:
>    - `Index()` GET — shows a search form with two fields: CPR Number and Patient Reference Number. No login required (`[AllowAnonymous]`).
>    - `Index(string cpr, string referenceNumber)` POST — calls `GET /api/appointments/lookup?cpr={cpr}&ref={referenceNumber}` using `HttpClient`. Displays results or an error message.
>
> 2. `LookupService.cs` — registered as a typed HttpClient service in `Program.cs`. Base address points to the ClinicAPI URL (from appsettings). Calls the API and deserializes JSON response into view models.
>
> 3. `Views/Lookup/Index.cshtml` — public page (no login link needed), shows:
>    - The search form
>    - If results found: upcoming appointments (doctor name, specialty, date/time, status) and last 3 visit summaries (date, diagnosis)
>    - If not found: friendly 'No records found' message
>    - If API error: friendly error message
>
> 4. Add a link to this page in the `_Layout.cshtml` navbar as 'Check My Appointment' (visible to everyone including not logged in).
>
> In `ClinicMVC/appsettings.json`, add:
> `'ApiBaseUrl': 'https://localhost:{ClinicAPI port}'`
>
> IMPORTANT: This is the ONLY feature in ClinicMVC that calls the API. All other features use EF Core directly."

- [ ] Create all files
- [ ] Run both `ClinicAPI` and `ClinicMVC` simultaneously (right-click solution → Set Startup Projects → Multiple startup projects)
- [ ] Go to the public lookup page WITHOUT logging in
- [ ] Enter the CPR and reference number from your seed data → confirm appointments appear
- [ ] Enter wrong data → confirm friendly error message

---

### Step 29 — `ALI`
**Commit public lookup page**

```bash
git add .
git commit -m "Implement public appointment lookup page using HttpClient to call ClinicAPI"
git push origin main
```

---

## STAGE 11 — SignalR Real-Time Appointment Tracking (Member 4)

---

### Step 30 — `AI AGENT`
**Prompt to give the AI:**

> "Implement SignalR real-time appointment tracking in the ClinicSystem. I need:
>
> In `ClinicAPI`:
> 1. `AppointmentHub.cs` in `ClinicAPI/Hubs/` — a SignalR Hub class. It should have a method `JoinWaitingRoom()` that adds the client to a group. Configure it in `Program.cs` at route `/hubs/appointment`.
> 2. Inject `IHubContext<AppointmentHub>` into `AppointmentController` (the MVC one). In the `UpdateStatus` action, after every status change, call: `await _hubContext.Clients.All.SendAsync('AppointmentStatusUpdated', new { appointmentId, patientName, doctorName, newStatus, updatedAt })`
>
> In `ClinicMVC`:
> 3. Install `Microsoft.AspNetCore.SignalR.Client` NuGet package in ClinicMVC
> 4. Create `Views/Receptionist/WaitingRoom.cshtml` — a live waiting room board showing today's appointments as cards. Each card shows: patient name, appointment time, doctor name, current status (color-coded). When a `AppointmentStatusUpdated` message is received via SignalR, update the relevant card's status badge without page refresh.
> 5. Add the SignalR JavaScript client script to this view. The connection URL should point to `ClinicAPI`'s hub: `https://localhost:{APIport}/hubs/appointment`
> 6. Add a 'Waiting Room' link in the Receptionist's navigation menu.
> 7. Also wire up personal notifications: when a notification arrives for the current user via SignalR, play a subtle sound and update the bell badge count immediately.
>
> Configure CORS in `ClinicAPI/Program.cs` to allow the MVC app's origin."

- [ ] Create all files, update Program.cs in both projects
- [ ] Run both ClinicAPI and ClinicMVC simultaneously
- [ ] Open the Waiting Room view in one browser tab
- [ ] In another tab (as Receptionist), change an appointment status
- [ ] Confirm the Waiting Room card updates instantly without refreshing

---

### Step 31 — `ALI`
**Commit SignalR feature**

```bash
git add .
git commit -m "Implement SignalR real-time waiting room and live appointment status updates"
git push origin main
```

---

## STAGE 12 — Reporting Application (Member 4)

---

### Step 32 — `AI AGENT`
**Prompt to give the AI:**

> "Implement the Reporting Application (`ClinicReporting` project — ASP.NET Core MVC). This app has NO project reference to ClinicAPI and NO direct database access. All data comes through the API via HttpClient.
>
> 1. `AuthController.cs`:
>    - `Login()` GET+POST — form with email+password. On POST, call `POST /api/auth/login` via HttpClient. If successful, store the JWT token in the session. Redirect to dashboard.
>    - `Logout()` — clear session, redirect to login.
>
> 2. `ApiService.cs` — a typed HttpClient service that:
>    - Reads the JWT from the session on every request
>    - Attaches `Authorization: Bearer {token}` header
>    - Has methods: `GetAppointmentStatsAsync(DateTime from, DateTime to)`, `GetDoctorUtilizationAsync(DateTime from, DateTime to)`, `GetCancellationRatesAsync(DateTime from, DateTime to)`
>
> 3. `ReportsController.cs`:
>    - `Dashboard()` — overview page with summary numbers (total appointments today, this week, this month)
>    - `AppointmentStats()` — date range filter form + a table and simple bar chart of appointments by status
>    - `DoctorUtilization()` — table showing each doctor's total appointments, completed, cancelled, completion rate
>    - `CancellationRates()` — table of cancellation and missed rates with date filter
>    - All actions check if JWT exists in session — redirect to login if not
>    - All report pages have a 'Date Range' filter (from/to date pickers)
>
> 4. `_Layout.cshtml` for reporting app — clearly branded 'Clinic Manager — Reports', shows logged-in manager name, logout button, nav links to each report.
>
> 5. In `ClinicReporting/appsettings.json` add: `'ApiBaseUrl': 'https://localhost:{ClinicAPI port}'`
>
> 6. Configure session in `ClinicReporting/Program.cs`
>
> IMPORTANT: This app is read-only. No create/update/delete. Only the Clinic Manager role should be able to log in (validate the role from the JWT claims)."

- [ ] Create all files
- [ ] Run all three projects simultaneously
- [ ] Log in to the reporting app at its port with manager@clinic.com
- [ ] View all three report pages with real data
- [ ] Try logging in as a non-manager role — confirm it is blocked
- [ ] Try accessing a report page without logging in — confirm redirect to login

---

### Step 33 — `ALI`
**Commit reporting application**

```bash
git add .
git commit -m "Implement Reporting Application with JWT auth, HttpClient API calls, and operational reports"
git push origin main
```

---

## STAGE 13 — UI Polish & Quality Pass (Member 4)

---

### Step 34 — `AI AGENT`
**Prompt to give the AI:**

> "Review and improve the UI/UX of the ClinicMVC application. I need:
>
> 1. A consistent Bootstrap 5 layout across all pages — use a clean, professional medical theme (white background, blue/teal primary color).
> 2. Update `_Layout.cshtml` with a proper navbar, sidebar (for dashboards), and footer.
> 3. Add Bootstrap badges with appropriate colors for appointment statuses:
>    - Requested = secondary, Confirmed = primary, CheckedIn = info, InProgress = warning, Completed = success, Cancelled = danger, Missed = dark
> 4. Add DataTables.js to all list pages (appointments, patients, doctors) for client-side search, sort, and pagination.
> 5. Add a date picker (Flatpickr or similar) to all date input fields.
> 6. Add SweetAlert2 for confirmation dialogs before status changes (e.g., 'Are you sure you want to cancel this appointment?').
> 7. Add a loading spinner on form submissions that make API calls.
> 8. Make all pages responsive — test the layout at 768px width (tablet) and 375px (mobile).
> 9. Add a 404 and 500 error page.
> 10. Add `[ValidateAntiForgeryToken]` to all POST actions if not already there.
>
> Give me updated `_Layout.cshtml`, a `site.css` customization file, and updated views for the main list and form pages."

- [ ] Apply the UI updates
- [ ] Test every page at different screen widths using browser DevTools (F12 → Toggle device toolbar)
- [ ] Confirm all forms show validation errors when submitted empty
- [ ] Confirm confirmation dialogs appear before destructive actions

---

### Step 35 — `AI AGENT`
**Prompt to give the AI:**

> "Add server-side input validation and error handling to ClinicMVC. I need:
> 1. All ViewModels should have complete `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[DataType]` validation annotations.
> 2. All POST controller actions should check `ModelState.IsValid` before processing.
> 3. A global exception handling middleware in `Program.cs` that catches unhandled exceptions and shows the 500 error page.
> 4. All database calls wrapped in try/catch with meaningful error messages via TempData.
> 5. Authorization checks — if a user tries to access another user's appointment/record via URL manipulation, return 403 Forbidden.
> 6. Verify all list pages have null-safe `.ToList()` calls (no NullReferenceException on empty data).
>
> List all the files that need changes and what to change in each."

- [ ] Apply all validation and error handling changes
- [ ] Test: submit empty forms → confirm validation messages appear
- [ ] Test: log in as Patient, manually type a URL for the Manager dashboard → confirm redirect/403

---

### Step 36 — `ALI`
**Commit UI polish and validation**

```bash
git add .
git commit -m "Add UI polish, Bootstrap styling, DataTables, SweetAlert2, validation, and error handling"
git push origin main
```

---

## STAGE 14 — Enhancements (Bonus 5%) (Member 4)

---

### Step 37 — `AI AGENT`
**Prompt to give the AI (choose 2–3 of these):**

> "Add the following enhancements to the ClinicSystem to go beyond the baseline requirements. For each enhancement, give me the complete code and explain how to test it:
>
> Enhancement 1 — Appointment Reminder Email Simulation: When an appointment is 24 hours away, display a banner on the Patient's dashboard: 'Reminder: You have an appointment tomorrow at [time] with Dr. [name]'. Check this on dashboard load using a LINQ query.
>
> Enhancement 2 — Doctor Dashboard Calendar View: Instead of a plain list, show the Doctor's appointments for the week in a simple calendar grid (a 5-column table Mon–Fri with time slots). Use color coding by appointment status.
>
> Enhancement 3 — Patient Search for Receptionist: Add a patient search bar on the Receptionist's 'Book Appointment' page. As the receptionist types a patient name or CPR, AJAX returns matching patients without page refresh. Uses `GET /api/patients/search?q={query}` endpoint.
>
> Enhancement 4 — Appointment Export: Clinic Manager can export the appointments list as a CSV file. Add an 'Export CSV' button on the All Appointments page.
>
> Enhancement 5 — Prescription Print View: A print-friendly prescription page at `/prescription/{id}/print` with clinic letterhead, patient details, doctor details, medication table, and doctor signature line. Uses `@media print` CSS."

- [ ] Implement chosen enhancements
- [ ] Take a screenshot of each enhancement working for the project document

---

### Step 38 — `ALI`
**Commit enhancements**

```bash
git add .
git commit -m "Add enhancements: [list the ones you added]"
git push origin main
```

---

## STAGE 15 — Azure Deployment (Member 4)

---

### Step 39 — `ALI`
**Receive Azure credentials from the polytechnic**

- [ ] Contact your tutor/coordinator to receive your group's Azure account credentials
- [ ] Log in to the Azure Portal at portal.azure.com
- [ ] Confirm you can see the portal dashboard

---

### Step 40 — `ALI`
**Create Azure resources**

In the Azure Portal:
- [ ] Click "Create a resource group" → name it: `clinic-system-rg` → region: closest to Bahrain (e.g., UAE North or West Europe)
- [ ] Create **Azure SQL Server**:
  - Name: `clinic-sql-server-[yourname]`
  - Admin username: `clinicadmin`
  - Admin password: choose a strong one and write it down
  - Region: same as resource group
- [ ] Create **Azure SQL Database** on that server:
  - Name: `ClinicSystemDB`
  - Pricing tier: Basic (cheapest)
- [ ] Copy the ADO.NET connection string from the database → you will need it
- [ ] Create **3 Azure App Services** (one for each project):
  - `clinic-api` → Runtime: .NET 9, OS: Windows
  - `clinic-mvc` → Runtime: .NET 9, OS: Windows
  - `clinic-reporting` → Runtime: .NET 9, OS: Windows
  - Pricing tier: Free F1 for all three
- [ ] Note the URL of each App Service (e.g., `https://clinic-api.azurewebsites.net`)

---

### Step 41 — `AI AGENT`
**Prompt to give the AI:**

> "I have created 3 Azure App Services and an Azure SQL Database for the ClinicSystem project. Give me:
>
> 1. The complete `appsettings.Production.json` for `ClinicAPI` — replace the connection string with Azure SQL, update JWT settings for production.
> 2. The complete `appsettings.Production.json` for `ClinicMVC` — update the ApiBaseUrl to point to the deployed `ClinicAPI` URL: `https://clinic-api.azurewebsites.net`
> 3. The complete `appsettings.Production.json` for `ClinicReporting` — update ApiBaseUrl to `https://clinic-api.azurewebsites.net`
> 4. Instructions for configuring the connection string as an Azure App Service Application Setting (so it does not get committed to Git).
> 5. Step-by-step instructions to publish each project from Visual Studio using the 'Publish' wizard to Azure App Service."

- [ ] Create the production appsettings files
- [ ] Add the Azure SQL connection string as an App Service Application Setting (via portal — not in the code file)

---

### Step 42 — `ALI`
**Deploy all three projects to Azure**

In Visual Studio:
- [ ] Right-click `ClinicAPI` → Publish → Azure → Azure App Service (Windows) → select `clinic-api` → Publish
- [ ] Wait for deployment → confirm `https://clinic-api.azurewebsites.net/swagger` loads
- [ ] Right-click `ClinicMVC` → Publish → Azure → select `clinic-mvc` → Publish
- [ ] Right-click `ClinicReporting` → Publish → Azure → select `clinic-reporting` → Publish

---

### Step 43 — `ALI`
**Run migrations and seed data on Azure SQL**

- [ ] In Visual Studio Package Manager Console, run:
  ```
  Update-Database -StartupProject ClinicAPI -Connection "your-azure-sql-connection-string"
  ```
  *(Or use the EF Core CLI: `dotnet ef database update --connection "..."` )*
- [ ] Open Azure Data Studio → connect to your Azure SQL Server
- [ ] Run `seed.sql` script against the Azure SQL database
- [ ] Verify the data is there (query the AspNetUsers table)

---

### Step 44 — `ALI`
**Test the deployed application**

- [ ] Open `https://clinic-mvc.azurewebsites.net` — confirm the login page loads
- [ ] Log in as Clinic Manager → confirm dashboard works
- [ ] Log in as Patient → book an appointment → confirm it appears
- [ ] Test the public lookup page on the deployed MVC app
- [ ] Open `https://clinic-reporting.azurewebsites.net` → log in as Manager → view reports
- [ ] Test SignalR on the deployed app (may need Azure SignalR Service — see next step if it does not work)

---

### Step 45 — `AI AGENT` (if SignalR does not work after deployment)
**Prompt to give the AI:**

> "SignalR is not working after deploying to Azure App Service. The Free tier (F1) does not support WebSockets. Give me:
> 1. Instructions to enable WebSockets on the Azure App Service (it is a setting in the portal — Configuration → General settings → WebSockets = On). Tell me exactly where to find it.
> 2. If WebSockets cannot be enabled on Free tier, show me how to configure SignalR to fall back to Long Polling by adding `.WithUrl(..., options => { options.Transports = HttpTransportType.LongPolling; })` in the JavaScript client."

- [ ] Apply the fix and re-test SignalR on the deployed app

---

### Step 46 — `ALI`
**Commit production config files**

```bash
git add .
git commit -m "Add production appsettings for Azure deployment"
git push origin main
```

> Note: Never commit the actual connection string with credentials. Use Azure App Service Application Settings for that.

---

## STAGE 16 — Documentation (Member 4)

---

### Step 47 — `AI AGENT`
**Prompt to give the AI:**

> "Generate the following content for my project document for IT8118 Brief A:
>
> 1. A complete API Endpoints Summary Table (Markdown format) with columns: Route, HTTP Method, Purpose, Auth Required, Role Restriction — for all 9 API endpoints we implemented.
>
> 2. A NuGet Packages List table with columns: Package Name, Project(s) Used In, Purpose — for all packages installed.
>
> 3. A description of all database entities (for the ERD section): for each entity, list the key fields and briefly explain its role in the system.
>
> 4. An Enhancements section description for the [enhancements you chose] — a paragraph per enhancement with what it does and why it adds value.
>
> Format everything ready to paste into a Word document."

---

### Step 48 — `ALI`
**Draw the ERD diagram**

- [ ] Go to dbdiagram.io (free, online)
- [ ] Use the entity descriptions from the AI to draw the ERD
- [ ] Show all relationships (1-to-many, many-to-many) with cardinality notation
- [ ] Export as PNG or PDF
- [ ] Insert into the project document

---

### Step 49 — `ALI`
**Fill in the project document (using tutor template)**

- [ ] Open the template provided by the tutor
- [ ] Section: ERD → paste the diagram + entity descriptions from AI
- [ ] Section: Deployed URLs → paste the 3 Azure URLs
- [ ] Section: Demo Credentials → fill in the table:

| Role | Email | Password |
|------|-------|----------|
| Clinic Manager | manager@clinic.com | Manager@123 |
| Doctor | doctor1@clinic.com | Doctor@123 |
| Receptionist | receptionist@clinic.com | Recept@123 |
| Patient | patient1@clinic.com | Patient@123 |

- [ ] Section: API Endpoints → paste table from AI
- [ ] Section: NuGet Packages → paste table from AI
- [ ] Section: System Walkthrough → take screenshots of every major page per role
- [ ] Section: Enhancements → paste from AI + add your screenshots
- [ ] Add the GitHub repository URL

---

### Step 50 — `ALI`
**Final submission**

- [ ] Do a final commit:
  ```bash
  git add .
  git commit -m "Final submission - all features complete"
  git push origin main
  ```
- [ ] Confirm the GitHub repository is **public** and all code is visible
- [ ] Submit on the Moodle/course portal:
  - Project document (PDF or Word)
  - GitHub repository URL
- [ ] Submit before: **30 May 2026, 11:55 PM**

---

## STAGE 17 — Individual Reflection (Ali Only — 10%) (Member 1/2/3/4)

> Due: Week 12 (24th–28th May 2026) — submit this BEFORE the project deadline so you do not forget.

---

### Step 51 — `ALI`
**Write the Contribution Table**

- [ ] Go through your Git commit history on GitHub
- [ ] For each significant feature you committed, create a row:

| Feature/Component | What I Did | Git Commit Hash |
|-------------------|-----------|-----------------|
| Doctor Management | Implemented CRUD for doctors and schedules, wrote ManagerController | `abc1234` |
| Appointment Booking | Implemented multi-step booking flow with availability checking | `def5678` |
| ... | ... | ... |

---

### Step 52 — `ALI`
**Write the Technical Decisions section (2–3 decisions)**

For each decision, write:
- **What the decision was** (e.g., "I chose to use EF Core's Fluent API instead of Data Annotations for relationships")
- **What alternatives I considered** (e.g., "Data Annotations are simpler but less powerful for complex relationships")
- **Why I chose this approach** (e.g., "Fluent API gives finer control over cascade delete behavior and composite keys")

Good decisions to write about:
1. How you handled the appointment double-booking prevention logic
2. Why the three-project architecture separates concerns the way it does
3. How JWT was configured and why Bearer tokens are used for the API

---

### Step 53 — `ALI`
**Write the Technical Challenge section (1 challenge)**

Write about a real problem you hit and solved. For example:
- SignalR CORS not working between MVC and API — how you fixed it
- EF Core migration failing — what the error was and how you resolved it
- Azure deployment connection string issue — how you debugged it

Include: the actual error message, the code that was wrong, what you changed, and why that fixed it.

---

### Step 54 — `ALI`
**Submit the individual reflection**

- [ ] Add screenshots of pages you worked on
- [ ] Add code snippets from your work with brief explanations
- [ ] Save as PDF
- [ ] Submit on Moodle/portal before the Week 12 deadline

---

### Step 55 — `ALI`
**Attend in-person observation with tutor**

Prepare to explain (without notes):
- [ ] How the booking flow works end-to-end (from Patient clicking "Book" to Appointment in DB)
- [ ] How EF Core relationships work in your models (what `.Include()` does)
- [ ] How JWT authentication works (what claims are, what Bearer means)
- [ ] How SignalR sends real-time updates (Hub, client connection, event handling)
- [ ] Why the Reporting App cannot access the DB directly (architecture rule)
- [ ] How appointment status transitions are validated (the state machine)
- [ ] Walk through any controller action you are asked about line-by-line

---

## Summary — All Steps at a Glance

| Stage | Steps | Who | What |
|-------|-------|-----|------|
| 1 — Setup | 1–6 | Member 1 | Install tools, create GitHub repo, create solution, first commit |
| 2 — Database | 7–12 | Member 1 | Entity classes, DbContext, migrations, seed data |
| 3 — Auth | 13–15 | Member 2 | Identity login, role dashboards, role-based access |
| 4 — Doctor Mgmt | 16-17 | Member 3 | Doctor profiles, specializations, schedules, leaves |
| 5 — Booking | 18-19 | Member 2 | Multi-step booking flow, availability check, notifications |
| 6 — Lifecycle | 20-21 | Member 2 | Status transitions, validation, status history |
| 7 — Records | 22-23 | Member 3 | Visit records, patient history, prescriptions |
| 8 — Notifications | 24-25 | Member 2 | In-system notifications, bell badge, auto-refresh |
| 9 — Web API | 26-27 | Member 1 | JWT Auth, reporting endpoints, DTOs, Swagger |
| 10 — Lookup | 28-29 | Member 4 | Public lookup page via HttpClient |
| 11 — Sig�31 | AI + Ali | Real-time waiting room and status board |
| 12 — Reporting | 32–33 | AI + Ali | Reporting app with JWT, HttpClient, 3 report views |
| 13 — Polish | 34–36 | AI + Ali | Bootstrap UI, DataTables, validation, error handling |
| 14 — Enhancements | 37–38 | AI + Ali | Bonus features for 5% extra marks |
| 15 — Azure | 39–46 | Ali (+ AI guidance) | Provision resources, deploy all 3 apps, seed Azure DB |
| 16 — Docs | 47–50 | Ali (+ AI content) | ERD, project document, submit on portal |
| 17 — Reflection | 51–55 | Ali ONLY | Contribution table, decisions, challenge, in-person |

**Total steps: 55**  
**Deadline: 30 May 2026, 11:55 PM (project) | 24–28 May 2026 (reflection)**

---

*Follow these steps in order and the project will be complete. Do not skip steps. Commit after every stage.*
