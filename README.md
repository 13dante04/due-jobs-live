# Due Jobs live coding starter

This is a small internal scheduling service. The team already has an API and a dashboard. Choose the take-home ticket pack or work through the live interview tickets with your interviewer. Both start from this exact repository. You may change interfaces and structure as needed.

## Requirements

- .NET SDK 8 or newer
- Node.js 20 or newer and npm (for the optional Angular portion)

## Build

From the repository root:

```bash
dotnet build DueJobs.sln --configuration Release
```

`DueJobs.sln` groups the API, unit tests, and component tests and can be opened in an IDE.
The Angular frontend is built separately:

```bash
cd web
npm ci
npm run build
```

## Run

From `api`:

```bash
dotnet run --project DueJobs.Api
```

The API runs at `http://localhost:5080`. `GET /jobs` starts with three example jobs. `POST /jobs`, `POST /jobs/{id}/cancel`, and `POST /jobs/process-due` already exist. Delivery calls go to an in-memory recording gateway, not the network. Data resets on restart. This service has known behavior gaps; the exercise tickets describe what the team needs fixed or added.

From `web` (optional):

```bash
npm install
npm start
```

Open `http://localhost:4200`. Angular uses local mock jobs and runs without the API.
The development proxy is available for HTTP requests to `/jobs` but is unused by
the mock data service.

## Backend tests

From the repository root:

```bash
dotnet test DueJobs.sln --configuration Release
```

Run one suite from `api`:

```bash
dotnet test DueJobs.Tests.Unit
dotnet test DueJobs.Tests.Component
```

Both suites use xUnit and work with IDE test discovery. Unit tests exercise the
job service directly with a fixed clock and local in-memory collaborators.
Component tests send HTTP requests through ASP.NET Core's in-memory test host,
covering routing, JSON serialization, validation, persistence, cancellation, and
an email-processing workflow. Each component test owns a fresh host/repository,
a controllable clock, and a recording delivery gateway. No running API, database,
external delivery service, or Docker is needed.

Component tests disable the demo jobs with `SeedExampleJobs=false`; normal runs
still seed the three examples.

These are starter tests, not exhaustive coverage. Interview changes should add
focused tests for the requested behavior, including relevant scheduling boundaries,
delivery failures, and job-state transitions.

## Layout

- `api/DueJobs.Api`: controller API, model, clock, and in-memory repository
- `api/DueJobs.Tests.Unit`: xUnit service behavior tests
- `api/DueJobs.Tests.Component`: xUnit HTTP tests using `WebApplicationFactory`
- `web`: Angular dashboard with a routed app shell, jobs feature, typed mock data service,
  and reusable UI components; see [frontend documentation](web/README.md)

There is no database, authentication, Docker requirement, or hidden setup. The starter is intentionally modest: work on the requested tickets rather than rewriting the service.

## Demo branches

- `demo/controllers`: controller-based API with the existing routes and JSON contracts.
- `demo/fluent-validation`: branches from `demo/controllers`; moves creation validation into an injected FluentValidation validator (single responsibility and dependency inversion).
- `demo/database`: branches from `demo/controllers`; replaces in-memory storage with SQLite through the existing repository interface (dependency inversion).

Compare either sibling against `demo/controllers` to isolate its changes.

### FluentValidation demo

`CreateJobValidator` owns creation rules; `JobService` depends on
`IValidator<CreateJobRequest>`. Validation runs before saving, including for callers
outside HTTP. The clock is evaluated for each validation, not when DI constructs
the validator. The controller keeps the existing 400 `{ "error": "..." }` response.
Compare `git diff demo/controllers..demo/fluent-validation`.

Uses [FluentValidation's injected validator pattern](https://docs.fluentvalidation.net/en/latest/di.html).
