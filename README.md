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
| Backend API | In progress — all eight resources, JWT authentication |
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
│   ├── PetStore.Tests/            xUnit
│   └── Dockerfile                 Two-stage build for the API image
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
| `UnauthorizedException` | 401 | Credentials or token are invalid |
| `TooManyRequestsException` | 429 | The login rate limit has been reached |

### Validation

Format and range rules live on the DTOs as data annotations and are enforced before a
request reaches the controller. Rules that need context — whether a `BreedId` exists,
whether a birth date is in the future, whether a pet is still available — are checked in
the service layer.

### Transactions

Operations that span several tables are written in a single `SaveChangesAsync` call, so
they either all commit or none do. Placing an order inserts one `Order` row, N
`OrderItem` rows and updates N `Pet` rows atomically.

### Authentication

Passwords are hashed with BCrypt. Logging in returns a short-lived access token (15
minutes) and a refresh token (7 days). Refresh tokens are stored as SHA-256 digests
rather than in plain text, and each one is single-use: refreshing revokes the old token
and issues a new one, so a stolen token stops working as soon as the real user refreshes.

Who a request belongs to is read from the token, never from the request body. A customer
cannot place an order, read a favourite list or cancel an order on someone else's behalf,
even with a valid token of their own.

Login is rate limited in two layers: five failed attempts per email and twenty requests
per IP address, both over a five-minute sliding window. The first protects one account
from being guessed, the second stops one attacker from working through many accounts.

---

## Getting Started

### Everything in Docker

The only prerequisite is Docker Desktop.

Copy `.env.example` to `.env` and fill in the two secrets — the database password and the
JWT signing key. The key has to be at least 32 bytes; generate one with:

```powershell
$bytes = [byte[]]::new(32)
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

```bash
POSTGRES_DB=petstore
POSTGRES_USER=petstore_user
POSTGRES_PASSWORD=<your password>
POSTGRES_PORT=5432
JWT_KEY=<generated key>
```

Then:

```bash
docker compose up --build
```

Swagger UI: `http://localhost:8080/swagger`

The API applies any pending migrations on startup, so there is no separate migration
step. Nothing from `.env` reaches the repository — the file is git-ignored, and
`appsettings.json` ships with the key left empty. The application refuses to start if the
key is missing or too short, so a misconfiguration fails immediately rather than at the
first login attempt.

Uploaded photos live in a named volume, so they survive `docker compose down`.
`docker compose down -v` removes the volumes and resets everything.

### Running the API locally

Useful while developing, since the code reloads without rebuilding an image. Needs the
.NET 10 SDK and `dotnet tool install --global dotnet-ef` on top of Docker.

Start only the database:

```bash
docker compose up -d postgres
```

Secrets come from .NET user-secrets rather than `.env`, and never enter the repository:

```bash
cd backend
dotnet user-secrets init --project PetStore.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=petstore;Username=petstore_user;Password=<your password>" \
  --project PetStore.Api
dotnet user-secrets set "Jwt:Key" "<generated key>" --project PetStore.Api
```

Then apply migrations and run:

```bash
dotnet ef database update --project PetStore.Infrastructure --startup-project PetStore.Api
cd PetStore.Api
dotnet run
```

Swagger UI: `http://localhost:5087/swagger`

### Seed data

On first run in the Development environment the application seeds a small set of sample
species, breeds and pets, so the API is usable straight away. It also creates an admin
account — `admin@petstore.local` / `Admin123!` — for development only; the seeder never
runs outside Development, and in a real deployment the first admin has to be created
directly in the database. The seeder is skipped if the database already holds data.

---

## API

### Auth

| Method | Route | Description |
|---|---|---|
| POST | `/api/Auth/register` | 201 — creates a `Customer` account |
| POST | `/api/Auth/login` | 200 — access token + refresh token, 401 / 429 |
| POST | `/api/Auth/refresh` | 200 — rotates the refresh token, 401 |
| POST | `/api/Auth/logout` | 204 — revokes the refresh token |
| GET | `/api/Auth/me` | 200 — the caller's id, email and role |
| POST | `/api/Auth/change-password` | 204 — also revokes every refresh token |

Roles are never taken from the request: registering always produces a `Customer`, and
admin accounts are created through `POST /api/User`, which is itself admin-only.

### Access levels

Reading the catalogue — species, breeds, pets and their photos — needs no token. Placing
and managing your own orders and favourites needs any valid token. Everything that writes
to the catalogue, and all of `/api/User`, needs an `Admin` token.

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

The order is placed for whoever the token belongs to — there is no `userId` in the
request body. Listing every order is admin-only; the other four operations work on the
caller's own orders, and an order belonging to someone else returns 404 rather than 403,
so order ids cannot be probed.

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

The `{userId}` in a favourites route has to match the caller's own id, otherwise the
request is rejected with 403 — an admin is the only exception.

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
- [x] File upload for pet photos
- [x] Background service to release expired reservations
- [x] JWT authentication — hashing, refresh rotation, rate limiting, role and ownership checks
- [x] Dockerfile for the API itself — `docker compose up` brings up database and API
- [ ] Complete the `docs/openapi.yaml` contract
- [ ] React web client
- [ ] AI service: `/generate-description`, `/recommend-pet`
- [ ] Test automation and CI