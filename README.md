# PetStore

A pet adoption platform built as a layered .NET 10 API, a React web client, and a Python
AI service that grounds its recommendations in real data.

Developed solo for personal growth, using VBT Yazılım's
[internship assignment](https://github.com/VB10/staj-2026-intern-assignments) as a
reference specification.

---

## Status

| Component | Status |
|---|---|
| Backend API | In progress — Species, Breed, User and Pet resources working |
| Web (React) | Planned |
| AI Service (FastAPI + RAG) | Planned |
| QA (API + E2E tests) | Planned |

---

## Project Structure

```
PetStore/
├── backend/          .NET 10 API (layered)
│   ├── PetStore.Domain/           Entities, interfaces, services
│   ├── PetStore.Infrastructure/   EF Core, DbContext, repositories
│   ├── PetStore.Api/              Controllers, DI, Swagger
│   └── PetStore.Tests/            xUnit
├── web/              React + TypeScript (planned)
├── ai/               Python + FastAPI (planned)
├── qa/               Test collections (planned)
├── docs/
│   ├── decisions.md  Technical decisions and their rationale
│   └── openapi.yaml  API contract
└── docker-compose.yml
```

---

## Architecture

The backend is split into four projects so that layer boundaries are enforced by the
compiler rather than by discipline alone: `PetStore.Domain` holds no reference to
`PetStore.Infrastructure`, so leaking data-access code into business logic produces a
build error.

```
PetStore.Api              HTTP, controllers, Swagger
       ↓
PetStore.Infrastructure   EF Core, DbContext, repository implementations
       ↓
PetStore.Domain           Entities, business rules, repository/service interfaces
                          (references nothing)
```

The path a request takes:

```
Controller → Service → IRepository → Repository → DbContext → PostgreSQL
```

Controllers never touch `DbContext` directly, and the service layer depends only on
interfaces.

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

### Steps

**1. Start the database**

Copy `.env.example` to `.env` and fill in `POSTGRES_PASSWORD`:

```bash
POSTGRES_DB=petstore
POSTGRES_USER=petstore_user
POSTGRES_PASSWORD=<your password>
POSTGRES_PORT=5432
```

Then:

```bash
docker compose up -d
```

**2. Store the connection string in user-secrets**

Connection details never enter the repository; they are supplied through .NET
user-secrets:

```bash
cd backend
dotnet user-secrets init --project PetStore.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=petstore;Username=petstore_user;Password=<your password>" \
  --project PetStore.Api
```

**3. Apply migrations**

```bash
dotnet ef database update --project PetStore.Infrastructure --startup-project PetStore.Api
```

**4. Run the API**

```bash
cd PetStore.Api
dotnet run
```

Swagger UI: `http://localhost:5087/swagger`

---

## API

Endpoints currently available:

| Method | Route | Description |
|---|---|---|
| GET | `/api/Species` | List species |
| GET | `/api/Species/{id}` | Get one species |
| POST | `/api/Species` | Create species |
| GET | `/api/Breed` | List breeds |
| GET | `/api/Breed/{id}` | Get one breed |
| POST | `/api/Breed` | Create breed |
| GET | `/api/Pet` | List pets |
| GET | `/api/Pet/{id}` | Get one pet |
| POST | `/api/Pet` | Create pet |
| GET | `/api/User` | List users |
| GET | `/api/User/{id}` | Get one user |
| POST | `/api/User` | Create user |

---

## Data Model

Eight tables: `Species`, `Breed`, `Pet`, `PetPhoto`, `User`, `Order`, `OrderItem`,
`Favorite`.

Two design decisions worth calling out:

**Species and breed are separate levels.** The reference API models a single flat
`Category`; this project splits it into `Species` and `Breed`. The reason is that the
attributes the AI recommendation relies on are only meaningful at breed level — "cat"
has no single energy level, "British Shorthair" does.

**Breed attributes are structured, not free text.** The `Breed` table stores seven
attributes as numeric fields on a 1–5 scale: `EnergyLevel`, `NoiseLevel`,
`SpaceRequirement`, `AppetiteLevel`, `GoodWithChildren`, `GoodWithOtherPets` and
`GroomingNeed`. Numeric scales were chosen over free-text descriptions so the AI service
can ground its recommendations in queryable data rather than interpreting prose.

Full decision log with alternatives considered: [`docs/decisions.md`](./docs/decisions.md)

---

## Roadmap

- [ ] Response DTOs — stop exposing entities directly through the API
- [ ] Centralised error handling (Problem Details / RFC 9457)
- [ ] Remaining resources: `PetPhoto`, `Order`, `OrderItem`, `Favorite`
- [ ] File upload for pet photos
- [ ] JWT authentication
- [ ] Complete the `docs/openapi.yaml` contract
- [ ] React web client
- [ ] AI service: `/generate-description`, `/recommend-pet`
- [ ] Test automation and CI
