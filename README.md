# Flash.SensitiveWords

Flash.SensitiveWords is a cleanly layered solution for sensitive word management and message filtering. It is implemented with ASP.NET Core, MSSQL, Swagger, and a thin MVC frontend that consumes the API through a dedicated REST client.

## What this solution delivers

- RESTful API for managing sensitive words and filtering chat messages
- Swagger/OpenAPI documentation for all endpoints, request models, and responses
- MSSQL-backed CRUD data layer with Entity Framework Core
- Separation of concerns across API, application, domain, infrastructure, REST client, and web UI layers
- ASP.NET MVC admin page for sensitive word management
- Mock chat page demonstrating how word filtering works without direct DB access from the frontend

## Project structure

- `src/Flash.SensitiveWords.API` - API host and minimal endpoint definitions
- `src/Flash.SensitiveWords.Application` - business rules and service layer
- `src/Flash.SensitiveWords.Domain` - domain entities and repository contracts
- `src/Flash.SensitiveWords.Infrastructure` - EF Core data access, SQL Server wiring, and database initialization
- `src/Flash.SensitiveWords.RestClient` - typed HTTP client wrappers for API consumption
- `src/Flash.SensitiveWords.Web` - ASP.NET MVC frontend for admin and chat experiences

## Key capabilities

- CRUD on sensitive words via `/sensitivewords`
- Message filtering via `/sensitivewords/filter`
- Swagger UI available when running the API in development
- Web UI uses REST client instead of direct database access

## Run the solution

1. Configure the SQL Server connection string in `src/Flash.SensitiveWords.API/appsettings.json` under `ConnectionStrings:DefaultConnection`.
2. Run the solution or start the API project.
3. The API will initialize the database and seed default sensitive words if needed.

## Swagger

- Swagger is configured in `src/Flash.SensitiveWords.API/Program.cs`
- In development, open the Swagger UI from the API host URL to inspect endpoints and request/response contracts

## Notes

- The MVC frontend is intentionally decoupled from the database; it interacts with the API through `Flash.SensitiveWords.RestClient`
- The architecture is designed for maintainability and easy extension across service, domain, and infrastructure boundaries
