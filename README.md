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
| Backend API | In progress — full CRUD for Species, Breed, User and Pet |
| Web (React) | Planned |
| AI Service (FastAPI + RAG) | Planned |
| QA (API + E2E tests) | Planned |

---

## Project Structure

```
PetStore/
├── backend/          .NET 10 API (layered)
│   ├── PetStore.Domain/           Entities, interfaces, services, DTOs
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
PetStore.Domain           Entities, business rules, DTOs,
                          repository/service interfaces
                          (references nothing)
```

The path a request takes:

```
Controller → Service → IRepository → Repository → DbContext → PostgreSQL
```

Controllers never touch `DbContext` directly, and the service layer depends only on
interfaces. Services return DTOs rather than entities, so the database schema and the
API contract can evolve independently.

### Error handling

A single `IExceptionHandler` translates domain exceptions into RFC 9457 Problem Details
responses, so controllers contain no try/catch blocks:

| Exception | Status | Raised when |
|---|---|---|
| `NotFoundException` | 404 | The requested record does not exist |
| `ValidationException` | 400 | A referenced record is missing, or a value fails a contextual rule |
| `ConflictException` | 409 | Deletion is blocked by dependent records |

### Validation

Format and range rules live on the DTOs as data annotations and are enforced before a
request reaches the controller. Rules that need context — whether a `BreedId` exists,
whether a birth date is in the future — are checked in the service layer.

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

On first run in the Development environment the application seeds a small set of sample
species, breeds and pets, so the API is usable straight away. The seeder is skipped if
the database already holds data.

---

## API

Every resource supports the same five operations:

| Method | Route | Returns |
|---|---|---|
| GET | `/api/{resource}` | 200 — list |
| GET | `/api/{resource}/{id}` | 200 — single record, 404 if missing |
| POST | `/api/{resource}` | 201 — created record, 400 on invalid input |
| PUT | `/api/{resource}/{id}` | 200 — updated record, 400 / 404 |
| DELETE | `/api/{resource}/{id}` | 204, 404 if missing, 409 if dependents exist |

Resources: `Species`, `Breed`, `Pet`, `User`.

### Deletion semantics

Deletion behaviour differs by entity, because the entities differ in role:

`Pet` and `User` are soft-deleted — the row is flagged rather than removed, since
`OrderItem` and `Favorite` reference pets and `Order` references users. A global query
filter hides flagged rows from every query, so a deleted record returns 404 on any
subsequent operation.

`Species` and `Breed` are deleted for real, but the service checks for dependent records
first and returns 409 rather than letting the database raise a foreign-key error.

---

## Data Model

Eight tables: `Species`, `Breed`, `Pet`, `PetPhoto`, `User`, `Order`, `OrderItem`,
`Favorite`.

Three design decisions worth calling out:

**Species and breed are separate levels.** The reference API models a single flat
`Category`; this project splits it into `Species` and `Breed`. The reason is that the
attributes the AI recommendation relies on are only meaningful at breed level — "cat"
has no single energy level, "British Shorthair" does.

**Breed attributes are structured, not free text.** The `Breed` table stores seven
attributes as numeric fields on a 1–5 scale: `EnergyLevel`, `NoiseLevel`,
`SpaceRequirement`, `AppetiteLevel`, `GoodWithChildren`, `GoodWithOtherPets` and
`GroomingNeed`. Numeric scales were chosen over free-text descriptions so the AI service
can ground its recommendations in queryable data rather than interpreting prose.

**Enums are stored as text.** `PetStatus`, `Gender`, `OrderStatus` and `UserRole` map to
text columns rather than integers, so the database is readable on its own and inserting a
new enum member cannot silently change what existing rows mean.

Full decision log with alternatives considered: [`docs/decisions.md`](./docs/decisions.md)

---

## Roadmap

- [x] Response DTOs — stop exposing entities directly through the API
- [x] Centralised error handling (Problem Details / RFC 9457)
- [x] Input validation
- [x] Development seed data
- [ ] Pagination, filtering and search
- [ ] Remaining resources: `PetPhoto`, `Order`, `OrderItem`, `Favorite`
- [ ] File upload for pet photos
- [ ] JWT authentication
- [ ] Dockerfile for the API itself
- [ ] Complete the `docs/openapi.yaml` contract
- [ ] React web client
- [ ] AI service: `/generate-description`, `/recommend-pet`
- [ ] Test automation and CI