# Quick Testing Guide - Web API Endpoints

## Test Credentials (from DataSeeder)

```
ClinicManager:
  Email: manager@medcenter.com
  Password: Manager@123

Receptionist:
  Email: receptionist@medcenter.com
  Password: Recept@123

Patient (for lookup test):
  Email: patient1@medcenter.com
  CPR: 860101001
  Reference: PAT-0001
```

## Testing Steps

### Step 1: Start the API
```bash
cd ClinicAPI
dotnet run
# API will be available at https://localhost:7053
```

### Step 2: Open Swagger
Navigate to: `https://localhost:7053/swagger`

### Step 3: Test Public Endpoint (No Auth Required)
1. Expand **AppointmentsController**
2. Click **GET /api/appointments/lookup**
3. Enter query params:
   - `cpr`: 860101001
   - `ref`: PAT-0001
4. Click "Try it out" ? "Execute"
5. ? Should return 200 with patient appointments and visit summaries

### Step 4: Get JWT Token
1. Expand **AuthController**
2. Click **POST /api/auth/login**
3. In request body, enter:
   ```json
   {
     "email": "manager@medcenter.com",
     "password": "Manager@123"
   }
   ```
4. Click "Execute"
5. Copy the `token` value from response

### Step 5: Authorize in Swagger
1. Click "Authorize" button (top right)
2. Paste token in format: `Bearer {token}`
3. Click "Authorize"
4. Click "Close"

### Step 6: Test Authenticated Endpoints

#### Test GET /api/appointments
1. Expand **AppointmentsController**
2. Click **GET /api/appointments**
3. Click "Execute"
4. ? Should return 200 with list of appointments

#### Test GET /api/doctors
1. Expand **DoctorsController**
2. Click **GET /api/doctors**
3. Click "Execute"
4. ? Should return 200 with doctor list

#### Test GET /api/doctors/{id}/availability
1. Expand **DoctorsController**
2. Click **GET /api/doctors/{id}/availability**
3. Enter `id`: 1 (or any valid doctor ID)
4. Enter `date`: 2025-05-15 (future date)
5. Click "Execute"
6. ? Should return 200 with available time slots

### Step 7: Test Role-Protected Endpoints

#### Test GET /api/reports/appointment-stats
1. Expand **ReportsController**
2. Click **GET /api/reports/appointment-stats**
3. Enter query params:
   - `from`: 2025-01-01
   - `to`: 2025-12-31
4. Click "Execute"
5. ? Should return 200 with statistics (manager@medcenter.com has ClinicManager role)

#### Test without Authorization
1. Click "Authorize" and clear the token
2. Try the same endpoint
3. ? Should return 401 Unauthorized

### Step 8: Test Receptionist Endpoint
1. Get token for receptionist@medcenter.com instead
2. Test **GET /api/appointments** ? Should work (Receptionist role)
3. Test **GET /api/reports/appointment-stats** ? Should fail with 403 Forbidden

## Expected Status Codes

| Scenario | Status |
|----------|--------|
| Valid request with proper auth | 200 ? |
| Missing required query params | 400 ? |
| No JWT token on protected endpoint | 401 ? |
| Insufficient role permissions | 403 ? |
| Resource not found | 404 ? |
| Server error | 500 ? |

## API Endpoints Summary

| Method | Endpoint | Auth | Role |
|--------|----------|------|------|
| POST | /api/auth/login | ? | - |
| GET | /api/appointments/lookup | ? | - |
| GET | /api/appointments | ? | Receptionist, ClinicManager |
| GET | /api/appointments/{id} | ? | Receptionist, ClinicManager |
| GET | /api/doctors | ? | Any |
| GET | /api/doctors/{id}/availability | ? | Any |
| GET | /api/reports/appointment-stats | ? | ClinicManager |
| GET | /api/reports/doctor-utilization | ? | ClinicManager |
| GET | /api/reports/cancellation-rates | ? | ClinicManager |

## All Endpoints Return JSON with Proper DTOs

? No entity classes exposed directly  
? All responses use DTO classes  
? Proper error messages included  
? Status codes follow REST conventions
