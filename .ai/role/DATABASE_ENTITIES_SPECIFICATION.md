# Database Entities & Enums Specification - Attendance Management System

This document specifies the structure of Entities and Enums for the Attendance Management System. The system uses SQL Server with data access implemented via ADO.NET.

## 1. Enums
Located in the `Domain/Enums` folder.
- `RoleEnum`: `Admin`, `GeneralManager`, `DepartmentManager`, `Employee`
- `Department`: `IT`, `HR`, `Finance`, `Sales`
- `Gender`: `Male`, `Female`, `Other`

## 2. Core Entities

### 2.1. Employee Information Group
- **Employee** (Base Class)
  - `Id` (Guid, Primary Key)
  - `FirstName` (string)
  - `LastName` (string)
  - `Gender` (Gender Enum)
  - `Department` (Department Enum)
  - `PhoneNumber` (string)
  - `IsIntern` (bool)
  - `Role` (RoleEnum)
- **Developer** (Inherits `Employee`)
  - `Band` (int)
  - `TechnicalDirection` (string)
- **QA** (Inherits `Employee`)
  - `Band` (int)
  - `CodingSkillsFlag` (bool)
- **Manager** (Inherits `Employee`)
  - `ManagerType` (RoleEnum - to distinguish between General Manager and Department Manager)

### 2.2. Attendance Information Group
- **AttendanceRecord**
  - `Id` (Guid, Primary Key)
  - `EmployeeId` (Guid, Foreign Key)
  - `Date` (DateTime, Date part only)
  - `ArrivalTime` (DateTime)
  - `DepartureTime` (DateTime?, Nullable for cases where check-out is missing)

## 3. Database Design Rules
- Since raw ADO.NET is used instead of EF Core, the `Employee` table should be designed using the **Table Per Hierarchy (TPH)** pattern: All employee types are stored in a single `Employees` table with an `EmployeeType` discriminator column (e.g., Dev, QA, Manager). Specific columns (Band, CodingSkillsFlag) should allow Nulls.
