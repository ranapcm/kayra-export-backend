# KayraExport Microservices Backend

KayraExport is a .NET 7 backend system built with Onion Architecture and a microservice-oriented design. It includes independent Auth, Product, Log, and API Gateway services.

Repository: https://github.com/ranapcm/kayra-export-backend

## Architecture

The system contains the following services:

### Auth Microservice

- ASP.NET Core Identity
- JWT access tokens
- Refresh token rotation
- Token revocation
- Role management
- Independent PostgreSQL database

Projects:

- `KayraExport.Auth.Core`
- `KayraExport.Auth.Application`
- `KayraExport.Auth.Infrastructure`
- `KayraExport.Auth.API`

### Product Microservice

- Onion Architecture
- CQRS with MediatR
- Product CRUD operations
- PostgreSQL persistence
- Redis caching
- Cache invalidation
- Role-based and policy-based authorization
- RabbitMQ event publishing

Projects:

- `KayraExport.Core`
- `KayraExport.Application`
- `KayraExport.Infrastructure`
- `KayraExport.API`

### Log Microservice

- Centralized product event storage
- RabbitMQ event consumer
- Structured JSON payload storage
- Information, Warning, Error, and Critical severity support
- Independent PostgreSQL database
- Log query endpoint

Projects:

- `KayraExport.Log.Core`
- `KayraExport.Log.Application`
- `KayraExport.Log.Infrastructure`
- `KayraExport.Log.API`

### API Gateway

- YARP Reverse Proxy
- Central JWT validation
- Route-based authorization
- Fixed-window rate limiting
- Auth, Product, and Log service routing

Project:

- `KayraExport.Gateway`

## Technologies

- .NET 7
- ASP.NET Core Web API
- ASP.NET Core Identity
- Entity Framework Core
- PostgreSQL
- Redis
- RabbitMQ
- YARP Reverse Proxy
- MediatR
- JWT Authentication
- Serilog
- Swagger / OpenAPI
- Docker Compose
- xUnit
- Moq

## System Flow

```text
Client
  |
  v
API Gateway
  |
  +-- Auth API ---- Auth PostgreSQL
  |
  +-- Product API - Product PostgreSQL
  |       |
  |       +-------- Redis
  |       |
  |       +-------- RabbitMQ
  |                     |
  |                     v
  +-- Log API ----- Log PostgreSQL
```

## Main Features

- User registration and login
- JWT access-token generation
- Refresh-token rotation and revocation
- Role-based authorization
- Policy-based authorization
- Product create, read, update, and delete operations
- CQRS command/query separation with MediatR
- Asynchronous database operations
- Redis product-list caching
- Cache invalidation after product changes
- RabbitMQ product-created and product-updated events
- Central event logging
- Structured JSON log payloads
- Log severity levels
- Gateway-level JWT validation
- Gateway-level rate limiting
- Global exception handling with Problem Details
- Swagger API documentation
- Unit tests

## Authorization Policies

| Policy | Allowed roles | Operations |
|---|---|---|
| `ProductRead` | Authenticated users | List and retrieve products |
| `ProductWrite` | `User`, `Admin` | Create and update products |
| `ProductDelete` | `Admin` | Delete products |

## Prerequisites

- .NET 7 SDK
- Docker Desktop
- Git
- Entity Framework Core CLI tools

Install the EF Core CLI tool if it is not already installed:

```bash
dotnet tool install --global dotnet-ef --version 7.0.11
```

## Infrastructure Services

Docker Compose starts the following infrastructure:

| Service | Container | Host port |
|---|---|---:|
| Product PostgreSQL | `kayra-postgres` | 5432 |
| Auth PostgreSQL | `kayra-auth-postgres` | 5433 |
| Log PostgreSQL | `kayra-log-postgres` | 5434 |
| Redis | `kayra-redis` | 6379 |
| RabbitMQ | `kayra-rabbitmq` | 5672 |
| RabbitMQ Management | `kayra-rabbitmq` | 15672 |

Start the infrastructure:

```bash
docker compose up -d
```

Check container health:

```bash
docker compose ps
```

RabbitMQ Management UI:

```text
http://localhost:15672
```

## JWT Configuration

The Auth API creates JWT tokens. The Product API and Gateway validate those tokens.

The same secure JWT key must be configured for all three projects. Do not commit the key to source control.

```bash
dotnet user-secrets set "Jwt:Key" "your-secure-key-at-least-32-characters" --project KayraExport.Auth.API

dotnet user-secrets set "Jwt:Key" "your-secure-key-at-least-32-characters" --project KayraExport.API

dotnet user-secrets set "Jwt:Key" "your-secure-key-at-least-32-characters" --project KayraExport.Gateway
```

The services use the following token values:

```text
Issuer: KayraExport.Auth
Audience: KayraExport.Services
```

Production secrets should be supplied through environment variables or a secure secret-management service.

## Database Migrations

Apply Product database migrations:

```bash
dotnet ef database update --project KayraExport.Infrastructure --startup-project KayraExport.API
```

Apply Auth database migrations:

```bash
dotnet ef database update --project KayraExport.Auth.Infrastructure --startup-project KayraExport.Auth.API
```

Apply Log database migrations:

```bash
dotnet ef database update --project KayraExport.Log.Infrastructure --startup-project KayraExport.Log.API
```

## Running the Services

Open a separate terminal for each service.

### Auth API

```bash
dotnet run --project KayraExport.Auth.API --no-launch-profile --urls "http://localhost:5227"
```

Swagger:

```text
http://localhost:5227/swagger/index.html
```

### Product API

```bash
dotnet run --project KayraExport.API --no-launch-profile --urls "http://localhost:5113"
```

Swagger:

```text
http://localhost:5113/swagger/index.html
```

### Log API

```bash
dotnet run --project KayraExport.Log.API --no-launch-profile --urls "http://localhost:5082"
```

Swagger:

```text
http://localhost:5082/swagger/index.html
```

### API Gateway

```bash
dotnet run --project KayraExport.Gateway --no-launch-profile --urls "http://localhost:5001"
```

Gateway health endpoint:

```text
http://localhost:5001/
```

## API Gateway Routes

| Route | Destination | Authorization |
|---|---|---|
| `/api/v1/auth/**` | Auth API | Anonymous |
| `/api/v1/products/**` | Product API | JWT required |
| `/api/v1/logs/**` | Log API | JWT required |

## Auth Endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/v1/auth/register` | Register a user |
| POST | `/api/v1/auth/login` | Log in and receive tokens |
| POST | `/api/v1/auth/refresh` | Rotate the refresh token |
| POST | `/api/v1/auth/revoke` | Revoke a refresh token |

## Product Endpoints

| Method | Endpoint | Policy |
|---|---|---|
| GET | `/api/v1/products` | `ProductRead` |
| GET | `/api/v1/products/{id}` | `ProductRead` |
| POST | `/api/v1/products` | `ProductWrite` |
| PUT | `/api/v1/products/{id}` | `ProductWrite` |
| DELETE | `/api/v1/products/{id}` | `ProductDelete` |

## Log Endpoint

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/logs?count=10` | Return the latest centralized event logs |

Example request through the Gateway:

```bash
curl "http://localhost:5001/api/v1/logs?count=10" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## Redis Caching

Product lists are cached using the `products:all` key.

The cache is invalidated after:

- Product creation
- Product update
- Product deletion

If Redis is temporarily unavailable, the Product API continues using PostgreSQL.

## Event-Driven Communication

The Product microservice publishes events to the durable RabbitMQ topic exchange:

```text
kayra.events
```

Routing keys:

```text
product.created
product.updated
```

The Log microservice consumes events from:

```text
kayra.logs.product-events
```

The consumer uses manual acknowledgements and stores structured event payloads in the Log PostgreSQL database.

## Logging

The system uses structured logging and centralized event storage.

Supported severity values:

- `Information`
- `Warning`
- `Error`
- `Critical`

Event payloads are stored as PostgreSQL `jsonb` values so they can be queried and analyzed.

## Error Handling

The Product API uses global exception-handling middleware and returns Problem Details responses.

Common response codes:

- `200 OK`
- `201 Created`
- `204 No Content`
- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict`
- `429 Too Many Requests`
- `500 Internal Server Error`

## Testing

Build the complete solution:

```bash
dotnet build
```

Run all unit tests:

```bash
dotnet test
```

The tests cover:

- Product creation
- Input normalization
- Repository persistence
- Cache invalidation
- Product event publishing


## Docker Images

Build each microservice image from the repository root:

```bash
docker build -f KayraExport.Auth.API/Dockerfile -t kayra-auth-api:1.0.0 .
docker build -f KayraExport.API/Dockerfile -t kayra-product-api:1.0.0 .
docker build -f KayraExport.Log.API/Dockerfile -t kayra-log-api:1.0.0 .
docker build -f KayraExport.Gateway/Dockerfile -t kayra-gateway:1.0.0 .
```

List the generated images:

```bash
docker images
```

Each image exposes port `8080`. Runtime settings such as database connections, RabbitMQ connections, service addresses, and the JWT key must be supplied through environment variables.

Example:

```bash
docker run --rm -p 5227:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e Jwt__Key="your-secure-production-key" \
  kayra-auth-api:1.0.0
```



Create release artifacts independently for each executable service:

```bash
dotnet publish KayraExport.Auth.API -c Release
dotnet publish KayraExport.API -c Release
dotnet publish KayraExport.Log.API -c Release
dotnet publish KayraExport.Gateway -c Release
```

Runtime configuration must be supplied through environment variables or a secure configuration provider. Database migrations must be applied as a separate administrative process before starting a new release.



## Twelve-Factor Practices

The project applies the following practices:

- Single version-controlled codebase
- NuGet-managed dependencies
- Environment-based secret configuration
- Independent backing services
- Separate build and runtime stages
- Stateless API processes
- Port-independent services
- Disposable background consumers
- Standard output logging
- Independent database migration commands

## Git Workflow

Development is performed on:

```text
test/v1.0.0
```

After final verification, the test branch is merged into:

```text
prod/v1.0.0
```

## Useful Commands

Check the repository:

```bash
git status
```

Check formatting problems:

```bash
git diff --check
```

Stop infrastructure containers:

```bash
docker compose stop
```

Remove stopped containers while preserving named volumes:

```bash
docker compose down
```