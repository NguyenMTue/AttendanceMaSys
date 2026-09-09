# Architecture Guidelines - Attendance Management System

This project is initialized based on the Clean Architecture principles. Since the system requirements specify a Web API and a Console Client, the project structure has been streamlined.

## 1. Components to Remove
- **Project Folders:** Remove `AppHost` and `ServiceDefaults` in the `src` directory.
- **Configuration Folders:** Remove the `.aspire` folder at the root directory.
- Remove any references to Aspire in the `.sln` file or `Directory.Build.props` (if present).

## 2. Layer Structure
- **Domain Layer (`/src/Domain`)**
  - **Responsibility:** Contains core Business Logic, Entities (`Employee`, `AttendanceRecord`), and Enums.
  - **Rules:** Strictly no references to infrastructure libraries or frameworks (e.g., no EF Core). Only pure C#.
- **Application Layer (`/src/Application`)**
  - **Responsibility:** Business rules, CQRS (if applied) or Services, and Interfaces (`IEmployeeRepository`, `IAttendanceRepository`).
  - **Rules:** Must not contain direct database access code.
- **Infrastructure Layer (`/src/Infrastructure`)**
  - **Responsibility:** Implements data access using ADO.NET (`SqlConnection`, `SqlCommand`) and handles file processing (e.g., Batch Import).
- **Web API Layer (`/src/Web`)**
  - **Responsibility:** Exposes RESTful APIs and handles Authentication/Authorization.
  - **Rules:** No core business logic. Receives HTTP Request -> Calls Application Layer -> Returns HTTP Response.
- **Console Client Layer (`/src/ConsoleClient` - To be created)**
  - **Responsibility:** Command Line Interface (CLI) for user interaction. Sends HTTP requests to the Web API using `HttpClient`.
