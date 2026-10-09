# Keyloop Service Scheduler

A vehicle service appointment scheduling application built with React, TypeScript, ASP.NET Core Web API, and SQL Server.

## Features

- View available dealerships and service types.
- Check appointment availability based on technician skills and service-bay availability.
- Book vehicle service appointments.
- View appointment details.
- Cancel existing appointments.
- Persist appointment data in SQL Server.
- Explore backend endpoints through Swagger UI.

## Technology Stack

- Frontend: React, TypeScript, Vite
- Backend: ASP.NET Core Web API, C#
- Database: SQL Server LocalDB
- ORM: Entity Framework Core
- API documentation: Swagger / OpenAPI
- Testing: xUnit, EF Core InMemory, SQLite in-memory
- Version control: Git and GitHub

## Prerequisites

Install the following tools:

- .NET 10 SDK
- Node.js and npm
- SQL Server LocalDB
- Visual Studio or Visual Studio Code
- Git

## Run the Backend

Open a terminal at the repository root and run:

```powershell
dotnet restore backend/KeyloopScheduler.slnx
dotnet run --project backend/KeyloopScheduler.Api
```

The API is expected to run at:

`http://localhost:5274`

Swagger UI:

`http://localhost:5274/swagger`

If your launch settings select a different port, use the URL shown in the terminal.

## Database Setup

The application uses SQL Server LocalDB for local development.

1. Ensure SQL Server LocalDB is installed and running.
2. Check the connection string in `backend/KeyloopScheduler.Api/appsettings.Development.json`.
3. Apply database migrations:

```powershell
dotnet ef database update --project backend/KeyloopScheduler.Api
```

If the EF Core command is unavailable, install the matching EF Core command-line tool first.

The project includes development seed data for demonstrating the booking workflow.

## Run the Frontend

Open a second terminal at the repository root:

```powershell
cd frontend
npm install
npm run dev
```

Open the Vite URL shown in the terminal, normally:

`http://localhost:5173`

Keep the backend running while using the frontend.

## Run Tests

From the repository root, run:

```powershell
dotnet test backend/KeyloopScheduler.slnx
```

The automated test suite currently contains 13 tests, covering:

- Date and time behavior.
- Appointment retrieval and missing-record handling.
- Booking validation, including past appointment times and unknown customers.
- Successful appointment creation.
- Technician and service-bay scheduling conflicts.
- Appointment cancellation and repeated cancellation.
- Allowing new bookings when previous appointments are cancelled.

The suite uses xUnit, EF Core InMemory for selected controller tests, and SQLite in-memory for relational booking integration tests.

Review the latest test output to confirm the actual pass/fail counts before submission.

## API Overview

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/customers` | Retrieve customers |
| GET | `/api/customers/{id}/vehicles` | Retrieve vehicles for a customer |
| GET | `/api/availability` | Check available technicians and service bays |
| GET | `/api/appointments` | Retrieve appointments |
| GET | `/api/appointments/{id}` | Retrieve an appointment |
| POST | `/api/appointments` | Create an appointment |
| PATCH | `/api/appointments/{id}/cancel` | Cancel an appointment |

Use Swagger UI to inspect the exact request and response contracts.

## Architecture

The React frontend calls the ASP.NET Core Web API. The backend validates booking requests, checks scheduling conflicts, and persists appointment information using Entity Framework Core and SQL Server.

See [`docs/SystemDesign.md`](docs/SystemDesign.md) for the system design, architecture diagram, data flow, technology choices, and observability strategy.

## AI Collaboration Narrative

### AI-Assisted Development Strategy

GenAI was used as a development assistant throughout the implementation of the Keyloop Service Scheduler. It helped break the assessment into manageable tasks, explain technical concepts, explore implementation approaches, and structure the application and documentation.

Development proceeded incrementally, with the existing project structure, technology stack, and assessment requirements guiding the implementation.

### Implementation and Verification

AI assistance was used to help develop and refine the React frontend, ASP.NET Core Web API, database integration, appointment booking workflow, and automated tests.

Suggestions were verified by building and running the application, exercising API endpoints through Swagger, checking the frontend in the browser, and running the automated test suite.

### Debugging and Quality Improvements

During testing, the EF Core InMemory provider exposed a limitation involving database transactions. The test setup was adjusted for the validation scenarios, and SQLite in-memory was introduced for relational integration tests.

The integration tests were expanded to cover successful booking, technician and service-bay scheduling conflicts, and cancellation-related behavior. A reported vulnerability in a transitive SQLite dependency was also investigated and addressed by updating the SQLite provider and rechecking the dependency tree.

### Developer Ownership

AI-generated suggestions were reviewed and tested rather than accepted automatically. Errors were investigated using compiler output and test results, and changes were made incrementally to preserve working functionality.

The developer remained responsible for reviewing the implementation, verifying the results, and deciding which changes to retain.

### Further Evidence

For the final submission, this narrative should be supplemented with representative prompts, examples of AI suggestions that required correction, and the corresponding verification results.

## Known Limitations and Future Improvements

Potential improvements include authentication and authorization, appointment notifications, broader automated test coverage, and production monitoring with centralized logs, metrics, tracing, and alerts.

These are future enhancements unless explicitly implemented in the current version.

## Assessment Demo

Add the presentation video link here when it is available. The demo should cover the application overview, booking workflow, cancellation workflow, design highlights, AI collaboration, and lessons learned.
