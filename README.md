## BPA1 – Blood Pressure Tracking System (ASP.NET Core MVC + EF Core + Identity)

  A production-style, full-stack blood pressure monitoring application built with ASP.NET Core MVC, Entity Framework Core, Identity authentication, and a dedicated service layer.
  Includes analytics dashboards, REST API endpoints, data filtering, category classification, and dev-ready CI/CD & containerization.

## Key Features

  🔐 User Authentication & Authorization
  
  - ASP.NET Core Identity (login, register, manage account)
  
  - Per-user isolation of blood pressure data
  
  - Secure cookie authentication
  
  📊 Blood Pressure Tracking
  
  - Create, edit, delete personal measurements
  
  - Capture systolic/diastolic, pulse, position (sitting/standing/lying), notes, and timestamps
  
  - Automatic blood pressure category classification
  (Normal, Elevated, Stage 1, Stage 2, Crisis)
  
  📈 Analytics Dashboard (Chart.js)
  
  - Trend Line Chart: daily systolic/diastolic values
  
  - Category Breakdown Pie Chart: distribution of BP categories
  
  - Backed by /api/measurements/trend and /api/measurements/categories
  
  🧩 Service & Data Architecture
  
  - Clean separation:
  
    - Controllers (MVC + API)
    
    - Service Layer (IMeasurementService)
    
    - EF Core DbContext
    
    - ViewModels for filtering, creation, pagination
  
  - Strong Model Validation using Data Annotations
  
  🧪 REST API Layer
  
  Available under /api/measurements:
  
  - List measurements (with filtering, sorting, pagination)
  
  - Create, update, delete (per authenticated user)
  
  - Category analytics
  
  - Trend analytics
  
  📦 Database & Persistence
  
  - EF Core (SQL Server)
  
  - ApplicationDbContext extends IdentityDbContext
  
    Tables:
    
    - BpMeasurements
    
    - Positions
  
    - Identity tables (AspNetUsers, etc.)
  
  ⚙️ Developer & Ops Features
  
  - Serilog structured console logging
  
  - Health Checks at /health
  
  - Docker Compose (Web + SQL Server)
  
  - GitHub Actions CI pipeline for build + test
  
  - Automatic DB seeding of:
  
    - Default user

    - Sample measurement data

    - Default positions: Sitting, Standing, Lying
   

## Project Structure Overview
```
/BPA1
│ Program.cs            → App startup (Identity, EF Core, Serilog, routing)
│ appsettings.json      → Base config
│ appsettings.Development.json → Dev config + Serilog
│
├── Data/
│   ├── ApplicationDbContext.cs → EF Core (BP tables + Identity)
│   ├── DbSeeder.cs             → Sample user + measurement seeding
│   └── SeedData.cs             → Alternative seeding helper
│
├── Models/
│   ├── ApplicationUser.cs
│   ├── BpMeasurement.cs        → Entity + category logic
│   └── ErrorViewModel.cs
│
├── ViewModels/
│   ├── MeasurementFilterVm.cs
│   ├── MeasurementListItemVm.cs
│   ├── MeasurementCreateVm.cs
│   └── PagedResult.cs
│
├── Services/
│   ├── IMeasurementService.cs
│   └── MeasurementService.cs   → Filtering, CRUD, analytics
│
├── Controllers/
│   ├── HomeController.cs
│   ├── BPMeasurementsController.cs  → MVC (HTML)
│   └── Api/MeasurementsController.cs → REST API (JSON)
│
├── Areas/Identity/Pages/Account
│   ├── Register.cshtml.cs
│   └── Register.cshtml
│
└── wwwroot/
    ├── css / js
    ├── lib/ (Bootstrap, jQuery, validation)
```
## REST API Overview

  Base path: /api/measurements
  
  Endpoints

  | Method | Endpoint                       | Description                              |
| ------ | ------------------------------ | ---------------------------------------- |
| GET    | `/api/measurements`            | List with filtering, sorting, pagination |
| GET    | `/api/measurements/trend`      | Timeseries for dashboard                 |
| GET    | `/api/measurements/categories` | Category breakdown                       |
| POST   | `/api/measurements`            | Create new measurement                   |
| PUT    | `/api/measurements/{id}`       | Update existing                          |
| DELETE | `/api/measurements/{id}`       | Delete                                   |

All endpoints require authentication.

## Health Check

  Check system health:

    /health


## Local Setup
  1. Requirements
  
  - .NET 6 SDK
  
  - SQL Server or LocalDB
  
  - Optional: Docker Desktop
  
  2. Restore & Build
     
    dotnet restore
    dotnet build
    
  4. Apply EF Core Migrations
  
  - If using migrations:
      
        dotnet tool install --global dotnet-ef
        dotnet ef migrations add InitialCreate
        dotnet ef database update
        
   If using the built-in seeders (default), the DB will self-populate at runtime.
  
  4. Run the Application

         dotnet run


## Visit:

  / → Home
  
  /Identity/Account/Register → Create account
  
  /BPMeasurements → Main measurement list
  
  /BPMeasurements/Dashboard → Trend & category charts

## Demo
- visit - https://bp-tracker-ckbje5gpgjfphxcu.canadacentral-01.azurewebsites.net/
- use email = test@yahoo.com password = Password@123

