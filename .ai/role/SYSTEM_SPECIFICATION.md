# System Specification - Attendance Management System

This document defines the functional groups and data flows of the Attendance Management System.

## 1. Authentication & Authorization
- **Goal:** Role-based access control (RBAC).
- **Workflow:** 
  - User logs in via the Web API.
  - API issues a Token (or Session) containing the Role information.
  - The Console Client attaches this Token to the Header to call secured APIs.

## 2. General Employee Features
- **Check-in / Check-out:** Record daily Arrival Time and Departure Time.
- **View Personal History:** Retrieve a list of personal attendance records, with optional date range filters (from date - to date).

## 3. Manager Features
- Inherits all privileges of a General Employee.
- **View Subordinate Data:** 
  - `Department Manager`: Web API returns only the list of employees and attendance data for staff in the same `Department`.
  - `General Manager`: Web API returns data for all employees in the company.

## 4. Administrator Features
- Inherits full access to view system data.
- **Batch Import:** 
  - Read a text or CSV file containing a list of employees using `StreamReader` in the Console Client.
  - Send the data payload to the Web API.
  - The API parses the data, creates accounts, and performs bulk inserts into the database using ADO.NET `SqlCommand`.

## 5. Client Interaction (Console Application)
- Dynamic Console Menus displayed based on the logged-in user's Role.
- Utilize `Console` libraries, `while` loops for navigation, and `try-catch` blocks to handle failed HTTP Requests gracefully.
- Display API response data (e.g., attendance history) in a tabular format within the console for better readability.
