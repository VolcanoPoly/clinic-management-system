# Web API Implementation - Complete Testing Guide

## Overview
This document provides comprehensive instructions for testing the Web API endpoints implemented in ClinicAPI.

## Implementation Details

### API Base URL
- **HTTPS**: `https://localhost:7053`
- **HTTP**: `http://localhost:5235`

### Controllers Implemented

#### 1. AuthController (`/api/auth`)
- **POST /api/auth/login** - Public endpoint for authentication
  - No JWT required
  - Returns JWT token with user info and roles

#### 2. AppointmentsController (`/api/appointments`)
- **GET /api/appointments/lookup** - Public appointment lookup
  - Query params: `cpr` (Patient CPR), `ref` (Reference number)
  - No JWT required
  - Returns upcoming appointments and last 3 visit summaries

- **GET /api/appointments** - Authenticated appointment list
  - JWT required (Receptionist or ClinicManager)
  - Query filters: `date`, `doctorId`, `status`

- **GET /api/appointments/{id}** - Appointment details
  - JWT required (Receptionist or ClinicManager)
  - Returns full appointment detail with status history

#### 3. DoctorsController (`/api/doctors`)
- **GET /api/doctors** - List all doctors
  - JWT required
  - Returns doctors with specializations

- **GET /api/doctors/{id}/availability** - Doctor availability
  - JWT required
  - Query param: `date` (appointment date)
  - Returns 30-minute available time slots

#### 4. ReportsController (`/api/reports`)
- **GET /api/reports/appointment-stats** - Appointment statistics
  - JWT required, ClinicManager role only
  - Query params: `from`, `to` (date range)
  - Returns: total count, breakdown by status, breakdown by specialization

- **GET /api/reports/doctor-utilization** - Doctor metrics
  - JWT required, ClinicManager role only
  - Query params: `from`, `to` (date range)
  - Returns: appointments per doctor, completion rate

- **GET /api/reports/cancellation-rates** - Cancellation statistics
  - JWT required, ClinicManager role only
  - Query params: `from`, `to` (date range)
  - Returns: cancellation count, missed count, rates over time

## Test Data

### User Accounts (Development Seeding)

#### ClinicManager
- Email: `manager@medcenter.com`
- Password: `Manager@123`
- Role: ClinicManager

#### Receptionist
- Email: `receptionist@medcenter.com`
- Password: `Recept@123`
- Role: Receptionist

#### Doctors (Password: `Doctor@123`)
- `doctor1@medcenter.com` - Omar Hassan (Cardiology, General Practice)
- `doctor2@medcenter.com` - Fatima Al-Zahra (General Practice, Pediatrics)
- `doctor3@medcenter.com` - Ahmed Al-Nouri (Neurology, General Practice)
- `doctor4@medcenter.com` - Sara Khalifa (Ophthalmology, ENT)
- `doctor5@medcenter.com` - Tariq Al-Sayed (Psychiatry)
- `doctor6@medcenter.com` - Maryam Jaffar (Gynecology, Dermatology)

#### Patients (Password: `Patient@123`)
- `patient1@medcenter.com` - Yousef Mansoor
  - CPR: `860101001`
  - Reference: `PAT-0001`
  - DOB: 1986-01-01
  - Blood Type: O+

- `patient2@medcenter.com` - Layla Qassim
  - CPR: `920515002`
  - Reference: `PAT-0002`
  - DOB: 1992-05-15
  - Blood Type: A+

## Step-by-Step Testing Guide

### Step 1: Start the API

1. Open ClinicAPI project in Visual Studio
2. Press **F5** to start debugging
3. Verify the application starts on `https://localhost:7053`
4. Check output window for "Application started"

### Step 2: Access Swagger UI

1. Open browser and navigate to: `https://localhost:7053/swagger`
2. Swagger should load with all endpoints visible
3. Verify the following sections:
   - auth
   - appointments
   - doctors
   - reports

### Step 3: Test Authentication (POST /api/auth/login)

1. In Swagger, find **auth** section
2. Click on **POST /api/auth/login**
3. Click "Try it out"
4. Enter request body:
   ```json
   {
     "email": "manager@medcenter.com",
     "password": "Manager@123"
   }
   ```
5. Click "Execute"
6. Expected Response: **200 OK**
   ```json
   {
     "success": true,
     "message": "Login successful",
     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
     "user": {
       "id": "...",
       "email": "manager@medcenter.com",
       "firstName": "Khalid",
       "lastName": "Al-Rashid",
       "roles": ["ClinicManager"]
     }
   }
   ```
7. **Copy the JWT token** (without quotes) for next steps

### Step 4: Authorize Swagger with JWT

1. Locate the green **Authorize** button at the top of Swagger
2. Click it
3. In the "Value" field, enter: `Bearer {paste_your_token_here}`
4. Click "Authorize"
5. Close the authorization dialog
6. You should now see a lock icon on endpoints that require auth

### Step 5: Test Public Appointment Lookup (GET /api/appointments/lookup)

1. Find **appointments** section
2. Click on **GET /api/appointments/lookup**
3. Click "Try it out"
4. Enter query parameters:
   - `cpr`: `860101001`
   - `ref`: `PAT-0001`
5. Click "Execute" (Note: This endpoint requires NO token)
6. Expected Response: **200 OK**
   ```json
   {
     "found": true,
     "message": "Patient found",
     "patientName": "Yousef Mansoor",
     "upcomingAppointments": [
       {
         "id": 1,
         "appointmentDateTime": "2024-12-20T10:00:00",
         "status": "Confirmed",
         "doctorName": "Omar Hassan",
         "specialization": "Cardiology",
         "notes": "Regular checkup"
       }
     ],
     "lastThreeVisits": [
       {
         "id": 1,
         "visitDate": "2024-12-10T10:00:00",
         "doctorName": "Omar Hassan",
         "specialization": "General Practice",
         "diagnosis": "Healthy",
         "treatment": "None required"
       }
     ]
   }
   ```

### Step 6: Test Authenticated Appointments List (GET /api/appointments)

1. Ensure you're authorized (from Step 4)
2. Click on **GET /api/appointments**
3. Click "Try it out"
4. Optional - Enter filter parameters:
   - `date`: (leave empty for all dates)
   - `doctorId`: (leave empty for all doctors)
   - `status`: (e.g., "Confirmed")
5. Click "Execute"
6. Expected Response: **200 OK** with array of appointments

### Step 7: Test Appointment Detail (GET /api/appointments/{id})

1. Click on **GET /api/appointments/{id}**
2. Click "Try it out"
3. Enter `id`: `1`
4. Click "Execute"
5. Expected Response: **200 OK** with detailed appointment including status history
   ```json
   {
     "id": 1,
     "patientId": 1,
     "patientName": "Yousef Mansoor",
     "doctorId": 1,
     "doctorName": "Omar Hassan",
     "specialization": "Cardiology",
     "appointmentDateTime": "2024-12-20T10:00:00",
     "status": "Confirmed",
     "notes": "Regular checkup",
     "cancellationReason": "",
     "statusHistory": [
       {
         "id": 1,
         "previousStatus": "Requested",
         "newStatus": "Confirmed",
         "changedAt": "2024-12-10T09:00:00",
         "changedByUserId": "..."
       }
     ]
   }
   ```

### Step 8: Test Doctors List (GET /api/doctors)

1. Click on **GET /api/doctors**
2. Click "Try it out"
3. Click "Execute"
4. Expected Response: **200 OK** with array of doctors and their specializations

### Step 9: Test Doctor Availability (GET /api/doctors/{id}/availability)

1. Click on **GET /api/doctors/{id}/availability**
2. Click "Try it out"
3. Enter parameters:
   - `id`: `1`
   - `date`: `2024-12-20` (or any date within business hours)
4. Click "Execute"
5. Expected Response: **200 OK**
   ```json
   {
     "doctorId": 1,
     "doctorName": "Omar Hassan",
     "date": "2024-12-20",
     "availableSlots": [
       {
         "startTime": "2024-12-20T08:00:00",
         "endTime": "2024-12-20T08:30:00",
         "available": true
       },
       {
         "startTime": "2024-12-20T08:30:00",
         "endTime": "2024-12-20T09:00:00",
         "available": false
       }
     ]
   }
   ```

### Step 10: Test Appointment Statistics (GET /api/reports/appointment-stats)

1. Login as manager (if not already authorized)
2. Click on **GET /api/reports/appointment-stats**
3. Click "Try it out"
4. Enter parameters:
   - `from`: `2024-01-01`
   - `to`: `2024-12-31`
5. Click "Execute"
6. Expected Response: **200 OK**
   ```json
   {
     "totalAppointments": 10,
     "byStatus": {
       "Confirmed": 5,
       "Completed": 3,
       "Cancelled": 2
     },
     "bySpecialization": {
       "Cardiology": 3,
       "General Practice": 5,
       "Pediatrics": 2
     }
   }
   ```

### Step 11: Test Doctor Utilization Report (GET /api/reports/doctor-utilization)

1. Click on **GET /api/reports/doctor-utilization**
2. Click "Try it out"
3. Enter date range (e.g., 2024-01-01 to 2024-12-31)
4. Click "Execute"
5. Expected Response: **200 OK** with doctor metrics

### Step 12: Test Cancellation Rates Report (GET /api/reports/cancellation-rates)

1. Click on **GET /api/reports/cancellation-rates**
2. Click "Try it out"
3. Enter date range
4. Click "Execute"
5. Expected Response: **200 OK** with cancellation statistics

### Step 13: Test Authorization Failures

#### Test 1: Missing JWT Token on Protected Endpoint
1. **Remove authorization** by clicking "Authorize" and then "Logout"
2. Try to access **GET /api/appointments**
3. Expected Response: **401 Unauthorized**

#### Test 2: Insufficient Permissions (Role-Based)
1. Login as Receptionist instead:
   - Email: `receptionist@medcenter.com`
   - Password: `Recept@123`
2. Try to access **GET /api/reports/appointment-stats**
3. Expected Response: **403 Forbidden** (Receptionist doesn't have ClinicManager role)

### Step 14: Test Error Handling

#### Invalid CPR/Reference
1. Call **GET /api/appointments/lookup** with:
   - `cpr`: `999999999`
   - `ref`: `INVALID`
2. Expected Response: **404 Not Found**
   ```json
   {
     "found": false,
     "message": "Patient not found with the provided CPR and reference number"
   }
   ```

#### Invalid Date Range
1. Call **GET /api/reports/appointment-stats** with:
   - `from`: `2024-12-31`
   - `to`: `2024-01-01`
2. Expected Response: **400 Bad Request**
   ```
   "'from' date must be before 'to' date"
   ```

## Troubleshooting

### API not starting
- Check SQL Server LocalDB is running
- Verify connection string in `appsettings.json`
- Check for port conflicts (API uses port 7053)

### Swagger not loading
- Ensure API is running
- Clear browser cache and try again
- Check that Swagger configuration in `Program.cs` is correct

### Authentication fails
- Verify test user credentials in DataSeeder
- Ensure database has been migrated and seeded
- Check JWT settings in `appsettings.json`

### 401 Unauthorized on authenticated endpoints
- Verify JWT token is included in Authorize header
- Check token hasn't expired (default: 60 minutes)
- Ensure Bearer prefix is present in authorization header

### 403 Forbidden on report endpoints
- Verify logged-in user has ClinicManager role
- Try logging in as `manager@medcenter.com`

## DTOs Created

All DTOs are located in `ClinicAPI/DTOs/` folder:

1. **LoginRequestDto.cs** - Login request
2. **LoginResponseDto.cs** - Login response with JWT
3. **UserInfoDto.cs** - User information
4. **AppointmentLookupDto.cs** - Public appointment lookup
5. **VisitSummaryDto.cs** - Visit record summary
6. **PatientLookupResponseDto.cs** - Patient lookup response
7. **AppointmentDto.cs** - Appointment data
8. **AppointmentStatusHistoryDto.cs** - Status history
9. **AppointmentDetailDto.cs** - Full appointment detail
10. **DoctorDto.cs** - Doctor information
11. **TimeSlotDto.cs** - Available time slots
12. **DoctorAvailabilityDto.cs** - Doctor availability
13. **AppointmentStatsDto.cs** - Appointment statistics
14. **DoctorUtilizationDto.cs** - Doctor metrics
15. **DoctorUtilizationReportDto.cs** - Doctor utilization report
16. **CancellationRateDataDto.cs** - Daily cancellation data
17. **CancellationRatesReportDto.cs** - Cancellation rates report

## Summary

All Web API endpoints have been implemented according to requirements:
- ? AuthController with JWT authentication
- ? AppointmentsController with public lookup and authenticated list/detail
- ? DoctorsController with availability calculation
- ? ReportsController with role-based access
- ? Proper HTTP status codes (200, 400, 401, 403, 404)
- ? All responses use DTOs (not entity classes)
- ? Swagger/OpenAPI configured at /swagger
- ? JWT configuration from appsettings.json
- ? CORS configured for MVC and Reporting apps

Ready for deployment and integration with ClinicMVC!
