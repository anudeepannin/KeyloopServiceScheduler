# AI Collaboration Narrative

## 1. Overview

AI tools were used as development assistants during the implementation of the Keyloop Service Scheduler. They helped with solution planning, code generation, debugging, test design, and documentation. All generated code and recommendations were reviewed and validated before being retained.

## 2. How AI Was Used

- **Architecture and planning:** Assisted in organizing the React frontend, ASP.NET Core Web API, and SQL Server database.
- **Backend development:** Assisted with appointment booking, availability checking, validation, cancellation, and conflict detection.
- **Frontend development:** Assisted with the booking form, appointment listing, dashboard, and navigation.
- **Testing:** Assisted in identifying core business scenarios and creating unit and integration tests.
- **Documentation:** Assisted in preparing the README, system design, setup instructions, and testing documentation.

## 3. Verification and Engineering Decisions

AI-generated suggestions were treated as proposals rather than automatically trusted solutions.

- Reviewed the booking flow and validation rules.
- Tested successful bookings and invalid booking requests.
- Added tests for technician and service-bay scheduling conflicts.
- Verified that cancelled appointments do not block new bookings.
- Used SQLite in-memory integration tests to exercise relational database behavior.
- Investigated and addressed a dependency vulnerability reported during package checks.
- Rebuilt the solution and ran the test suite to verify the changes.

## 4. Limitations and Lessons Learned

The EF Core InMemory provider does not support real database transactions in the same way as a relational database. Therefore, SQLite in-memory tests were used for relational booking scenarios. These tests do not replace concurrency testing against the actual SQL Server database.

AI-generated code can contain incorrect assumptions, incomplete validation, or dependency issues. Reviewing the code, examining test failures, and validating actual application behavior were necessary parts of the development process.

## 5. Developer Ownership

The developer remained responsible for architecture decisions, code review, testing, debugging, and final acceptance. AI assistance accelerated implementation and documentation, but application behavior was validated through builds, API checks, frontend testing, and automated tests.

## 6. Further Improvements

Future improvements include broader edge-case testing, SQL Server concurrency testing, structured logging and monitoring, authentication and authorization, and automated CI/CD validation.