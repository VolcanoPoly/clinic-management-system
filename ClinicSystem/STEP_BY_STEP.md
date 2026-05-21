# IT8118 — Complete Sequential Step-by-Step Guide
## Healthcare Clinic System (Brief A)

> Follow every step in order. Do not skip steps.  
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

### Step 4
**Create the three ASP.NET Core projects inside the solution**

- [x] Add `ClinicAPI` — ASP.NET Core Web API (.NET 9)
- [x] Add `ClinicMVC` — ASP.NET Core MVC (.NET 9), with a project reference to ClinicAPI
- [x] Add `ClinicReporting` — ASP.NET Core MVC (.NET 9), NO project reference to ClinicAPI

---

### Step 5 ✅ COMPLETED
**Install NuGet packages for all three projects**

- [x] Open **Tools → NuGet Package Manager → Package Manager Console** in Visual Studio
- [x] Install all required NuGet packages for each project
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

### Step 7
**Create all entity/domain classes**

- [x] Create a `Models` folder inside `ClinicAPI`
- [x] Create each entity class file inside that folder

---

### Step 8
**Write the ApplicationDbContext**

- [x] Create a `Data` folder inside `ClinicAPI`
- [x] Create `ApplicationDbContext.cs`

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
- [x] Confirm `Program.cs` in both `ClinicAPI` and `ClinicMVC` registers the DbContext (verify it is there)

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

### Step 11
**Create and run the seed script**

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

### Step 13
**Implement ASP.NET Core Identity authentication**

- [x] Create all files in the correct locations
- [x] Run the app (`F5` in Visual Studio) and test:
  - [x] Register a new patient account
  - [x] Log in with that account
  - [x] Log out

---

### Step 14
**Implement role-based dashboards**

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

### Step 16
**Implement Doctor and Specialization management**

- [x] Create all files
- [x] Log in as Clinic Manager and test:
  - [x] Create a new doctor
  - [x] Edit the doctor's specializations
  - [x] Set a weekly schedule
  - [x] Add a leave period
  - [x] Add a new specialization

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

### Step 18
**Implement the appointment booking feature**

- [x] Create all files
- [x] Test the full booking flow as a Patient
- [x] Test booking as a Receptionist (selecting a patient)
- [x] Try to book a slot that already exists — confirm it is blocked

---

### Step 19 — `ALI`
**Commit booking feature** ✅ COMPLETED

```bash
git add .
git commit -m "Implement multi-step appointment booking with availability checking and double-booking prevention"
git push origin main
```

---

## STAGE 6 — Core Feature: Appointment Lifecycle Management (Member 2)

---

### Step 20
**Implement appointment lifecycle management**

- [x] Create all files
- [x] Test the full lifecycle as each role:
  - [x] Patient books → Receptionist confirms → Receptionist checks in → Doctor starts → Doctor completes
  - [x] Try an invalid transition (e.g., Requested → Completed directly) — confirm it is blocked
  - [x] Patient cancels a Requested appointment — confirm it works
  - [x] Try cancelling a Completed appointment — confirm it is blocked

---

### Step 21 — `ALI`
**Commit lifecycle management** ✅ COMPLETED

```bash
git add .
git commit -m "Implement appointment lifecycle with status transitions, validation, and status history"
git push origin main
```

---

## STAGE 7 — Core Feature: Visit Records & Prescriptions (Member 3)

---

### Step 22
**Implement Visit Records and Prescriptions**

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

### Step 24
**Implement the in-system notifications feature**

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

### Step 26
**Implement the Web API endpoints**

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

### Step 28
**Implement the public appointment lookup page**

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

### Step 30
**Implement SignalR real-time appointment tracking**

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

### Step 32
**Implement the Reporting Application**

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

### Step 34
**Review and improve the UI/UX**

- [ ] Apply the UI updates
- [ ] Test every page at different screen widths using browser DevTools (F12 → Toggle device toolbar)
- [ ] Confirm all forms show validation errors when submitted empty
- [ ] Confirm confirmation dialogs appear before destructive actions

---

### Step 35
**Add server-side input validation and error handling**

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

### Step 37
**Implement bonus enhancements (choose 2–3)**

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

### Step 41
**Configure production appsettings for Azure**

- [ ] Create the production appsettings files for all three projects
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

### Step 45 (if SignalR does not work after deployment)
**Fix SignalR on Azure**

- [ ] Enable WebSockets on the Azure App Service: Portal → App Service → Configuration → General settings → WebSockets = On
- [ ] If WebSockets cannot be enabled on Free tier, configure SignalR to fall back to Long Polling in the JavaScript client
- [ ] Re-test SignalR on the deployed app

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

### Step 47
**Prepare project documentation content**

- [ ] Prepare the API Endpoints Summary Table (Route, HTTP Method, Purpose, Auth Required, Role Restriction)
- [ ] Prepare the NuGet Packages List (Package Name, Project, Purpose)
- [ ] Prepare entity descriptions for the ERD section
- [ ] Prepare Enhancements section description

---

### Step 48 — `ALI`
**Draw the ERD diagram**

- [ ] Go to dbdiagram.io (free, online)
- [ ] Draw the ERD with all entities and relationships
- [ ] Show all relationships (1-to-many, many-to-many) with cardinality notation
- [ ] Export as PNG or PDF
- [ ] Insert into the project document

---

### Step 49 — `ALI`
**Fill in the project document (using tutor template)**

- [ ] Open the template provided by the tutor
- [ ] Section: ERD → paste the diagram + entity descriptions
- [ ] Section: Deployed URLs → paste the 3 Azure URLs
- [ ] Section: Demo Credentials → fill in the table:

| Role | Email | Password |
|------|-------|----------|
| Clinic Manager | manager@clinic.com | Manager@123 |
| Doctor | doctor1@clinic.com | Doctor@123 |
| Receptionist | receptionist@clinic.com | Recept@123 |
| Patient | patient1@clinic.com | Patient@123 |

- [ ] Section: API Endpoints → paste table
- [ ] Section: NuGet Packages → paste table
- [ ] Section: System Walkthrough → take screenshots of every major page per role
- [ ] Section: Enhancements → paste + add your screenshots
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
| 11 — SignalR | 30-31 | Member 4 | Real-time waiting room and status board |
| 12 — Reporting | 32–33 | Member 4 | Reporting app with JWT, HttpClient, 3 report views |
| 13 — Polish | 34–36 | Member 4 | Bootstrap UI, DataTables, validation, error handling |
| 14 — Enhancements | 37–38 | Member 4 | Bonus features for 5% extra marks |
| 15 — Azure | 39–46 | Member 4 | Provision resources, deploy all 3 apps, seed Azure DB |
| 16 — Docs | 47–50 | Member 4 | ERD, project document, submit on portal |
| 17 — Reflection | 51–55 | Ali ONLY | Contribution table, decisions, challenge, in-person |

**Total steps: 55**  
**Deadline: 30 May 2026, 11:55 PM (project) | 24–28 May 2026 (reflection)**

---

*Follow these steps in order and the project will be complete. Do not skip steps. Commit after every stage.*
