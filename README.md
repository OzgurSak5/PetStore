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
| Backend API | In progress — all eight resources implemented |
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
| `ConflictException` | 409 | The resource's current state blocks the operation |

### Validation

Format and range rules live on the DTOs as data annotations and are enforced before a
request reaches the controller. Rules that need context — whether a `BreedId` exists,
whether a birth date is in the future, whether a pet is still available — are checked in
the service layer.

### Transactions

Operations that span several tables are written in a single `SaveChangesAsync` call, so
they either all commit or none do. Placing an order inserts one `Order` row, N
`OrderItem` rows and updates N `Pet` rows atomically.

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

`Species`, `Breed`, `Pet` and `User` each support the same five operations:

| Method | Route | Returns |
|---|---|---|
| GET | `/api/{resource}` | 200 — paged list |
| GET | `/api/{resource}/{id}` | 200 — single record, 404 if missing |
| POST | `/api/{resource}` | 201 — created record, 400 on invalid input |
| PUT | `/api/{resource}/{id}` | 200 — updated record, 400 / 404 |
| DELETE | `/api/{resource}/{id}` | 204, 404 if missing, 409 if dependents exist |

`GET /api/Pet` and `GET /api/Breed` accept filters alongside `pageNumber` and
`pageSize`. Pets can be filtered by status, breed, species, gender, price range and
vaccination, and searched by name. Breeds can be filtered by species and by their
attribute scales — maximum energy, noise and space requirement, minimum
child-friendliness. The breed filters are min/max rather than equality because a buyer
asks for "a calm pet", not "a pet whose energy level is exactly 2". The same parameters
will back the AI service's `recommend-pet` endpoint.

### Orders

| Method | Route | Description |
|---|---|---|
| GET | `/api/Order` | Paged list, optionally filtered by `userId` and `status` |
| GET | `/api/Order/{id}` | Single order with its items |
| POST | `/api/Order` | Place an order for one or more pets |
| PUT | `/api/Order/{id}/pay` | Mark as paid — pets become `Sold` |
| PUT | `/api/Order/{id}/cancel` | Cancel — pets return to `Available` |

Placing an order reserves each pet (`Pending`) for a configurable window and freezes its
price on the order item, so later price changes never alter past orders. Only orders in
`Created` status can be paid or cancelled.

### Favorites and pet photos

Both are scoped to their parent resource, since neither exists on its own:

```
GET    /api/User/{userId}/favorites
POST   /api/User/{userId}/favorites
DELETE /api/User/{userId}/favorites/{petId}

GET    /api/Pet/{petId}/photos
POST   /api/Pet/{petId}/photos
PUT    /api/Pet/{petId}/photos/{photoId}/primary
DELETE /api/Pet/{petId}/photos/{photoId}
```

Exactly one photo per pet is marked primary. The first photo added becomes the cover
automatically, and deleting the cover promotes the next one in sort order.

### Deletion semantics

Deletion behaviour differs by entity, because the entities differ in role:

`Pet` and `User` are soft-deleted — the row is flagged rather than removed, since
`OrderItem` and `Favorite` reference pets and `Order` references users. A global query
filter hides flagged rows from every query, so a deleted record returns 404 on any
subsequent operation.

`Species` and `Breed` are deleted for real, but the service checks for dependent records
first and returns 409 rather than letting the database raise a foreign-key error.

`Favorite` and `PetPhoto` are deleted outright — there is no history worth preserving.

---

## Data Model

Eight tables: `Species`, `Breed`, `Pet`, `PetPhoto`, `User`, `Order`, `OrderItem`,
`Favorite`.

Four design decisions worth calling out:

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

**Order item prices are frozen.** `OrderItem.Price` is a copy of `Pet.Price` taken when
the order is placed. Without it, repricing a pet would retroactively change the total of
every past order that included it.

Full decision log with alternatives considered: [`docs/decisions.md`](./docs/decisions.md)

---

## Roadmap

- [x] Response DTOs — stop exposing entities directly through the API
- [x] Centralised error handling (Problem Details / RFC 9457)
- [x] Input validation
- [x] Development seed data
- [x] Pagination, filtering and search
- [x] Remaining resources: `PetPhoto`, `Order`, `OrderItem`, `Favorite`
- [ ] File upload for pet photos
- [ ] Background service to release expired reservations
- [ ] JWT authentication
- [ ] Dockerfile for the API itself
- [ ] Complete the `docs/openapi.yaml` contract
- [ ] React web client
- [ ] AI service: `/generate-description`, `/recommend-pet`
- [ ] Test automation and CI