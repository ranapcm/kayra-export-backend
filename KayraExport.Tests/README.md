# KayraExport Backend API

KayraExport is a RESTful backend API developed with .NET 7 using Onion Architecture. It provides JWT-based authentication, product management, PostgreSQL persistence, Redis caching, global exception handling, structured logging, and unit tests.

## Technologies

- .NET 7
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Redis
- MediatR
- JWT Authentication
- Serilog
- Docker Compose
- xUnit
- Moq
- Swagger / OpenAPI

## Architecture

The solution follows Onion Architecture and is divided into the following projects:

- `KayraExport.Core`: Domain entities
- `KayraExport.Application`: Use cases, CQRS commands and queries, DTOs, and interfaces
- `KayraExport.Infrastructure`: Database, repositories, authentication, caching, and external service implementations
- `KayraExport.API`: Controllers, middleware, dependency configuration, and application startup
- `KayraExport.Tests`: Unit tests

## Features

- User registration and login
- JWT-based authentication
- Protected product endpoints
- Product create, read, update, and delete operations
- CQRS implementation with MediatR
- PostgreSQL persistence with Entity Framework Core
- Redis caching for product lists
- Cache invalidation after create, update, and delete operations
- Global exception handling with Problem Details responses
- Structured console logging with Serilog
- Swagger documentation and Bearer authentication
- Unit tests with xUnit and Moq

## Prerequisites

- .NET 7 SDK
- Docker Desktop
- Git

## Configuration

The development database and Redis services are configured in `docker-compose.yml`.

The JWT signing key must not be committed to source control. Configure it with .NET User Secrets:

```bash
dotnet user-secrets set "Jwt:Key" "your-secure-development-key-at-least-32-characters" --project KayraExport.API
```

## Running the Project

Start PostgreSQL and Redis:

```bash
docker compose up -d
```

Apply the database migrations:

```bash
dotnet ef database update --project KayraExport.Infrastructure --startup-project KayraExport.API
```

Run the API:

```bash
dotnet run --project KayraExport.API
```

Swagger is available at:

```text
http://localhost:5113/swagger
```

## Authentication

Register a user through:

```text
POST /api/v1/auth/register
```

Log in through:

```text
POST /api/v1/auth/login
```

Copy the returned JWT token and use the Swagger `Authorize` button to access protected product endpoints.

## API Endpoints

| Method | Endpoint | Description | Authentication |
|---|---|---|---|
| POST | `/api/v1/auth/register` | Register a user | No |
| POST | `/api/v1/auth/login` | Log in and receive a JWT | No |
| GET | `/api/v1/products` | List all products | Yes |
| GET | `/api/v1/products/{id}` | Get a product by ID | Yes |
| POST | `/api/v1/products` | Create a product | Yes |
| PUT | `/api/v1/products/{id}` | Update a product | Yes |
| DELETE | `/api/v1/products/{id}` | Delete a product | Yes |

## Redis Caching

The product list is cached in Redis for five minutes using the `products:all` key.

The cache is invalidated whenever a product is created, updated, or deleted. If Redis is temporarily unavailable, the API continues operating with PostgreSQL.

## Testing

Run all unit tests:

```bash
dotnet test
```

The tests verify product creation, repository persistence, cache invalidation, and input normalization.

## Useful Commands

Check running containers:

```bash
docker compose ps
```

Stop the containers:

```bash
docker compose stop
```

Build the solution:

```bash
dotnet build
```

## Response Codes

- `200 OK`: Successful request
- `201 Created`: Resource successfully created
- `204 No Content`: Resource successfully deleted
- `400 Bad Request`: Invalid request
- `401 Unauthorized`: Invalid credentials or missing/invalid token
- `404 Not Found`: Resource not found
- `409 Conflict`: Conflicting resource or operation
- `500 Internal Server Error`: Unexpected server error