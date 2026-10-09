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
- Testing: xUnit
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

From the repository root:

```powershell
dotnet test backend/KeyloopScheduler.slnx
```

The test command builds the solution and runs the automated tests included in the repository.

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

### Strategy

GenAI was used as a development assistant to break the assessment into manageable tasks, explore implementation approaches, explain unfamiliar concepts, and help structure code and documentation. Requests were guided by the existing scenario, project architecture, technology stack, and required acceptance criteria.

### Verification

AI-generated suggestions were reviewed against the existing implementation. The application was run locally, API behavior was inspected using Swagger, and the frontend workflow was checked in the browser. Automated tests were run to verify the behavior covered by the test suite.

### Refinement and Quality Ownership

When issues appeared, the relevant code and error output were reviewed before making changes. The implementation was refined incrementally, preserving working functionality while addressing identified issues. Final decisions and responsibility for the submitted code remained with the developer.

The narrative should be updated before submission to reflect the actual prompts used, specific issues encountered, test coverage, and verification steps completed during development.

## Known Limitations and Future Improvements

Potential improvements include authentication and authorization, appointment notifications, broader automated test coverage, and production monitoring with centralized logs, metrics, tracing, and alerts.

These are future enhancements unless explicitly implemented in the current version.
