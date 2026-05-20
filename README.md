# Flash.SensitiveWords

Flash.SensitiveWords is a cleanly layered solution for sensitive word management and message filtering. It is implemented with ASP.NET Core 10, MSSQL, Swagger, and a thin MVC frontend that consumes the API through a dedicated REST client.

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

- CRUD on sensitive words via `/sensitivewords` (requires API Key authentication)
- Message filtering via `/sensitivewords/filter` (no authentication required)
- Swagger UI available when running the API in development with security scheme documentation
- Web UI uses REST client instead of direct database access
- API Key authentication via `X-Api-Key` header for all CRUD operations
- Request logging middleware that tracks request metadata, response times, and exceptions with distributed tracing support
- XML-documented endpoints in Swagger for detailed API contract information

## Security

- **API Key Authentication**: All CRUD endpoints for sensitive word management (`POST`, `PUT`, `DELETE` on `/sensitivewords/**`) require an `X-Api-Key` header
- The API key is configured in `appsettings.json` under `ApiSettings:ApiKey`
- The message filtering endpoint (`POST /sensitivewords/filter`) is publicly accessible without authentication
- Swagger UI displays the security scheme requirements for reference

### ⚠️ Demo vs. Production Security

**Current Configuration (Demo/Development Only):**
- The API key, database connection string, and other sensitive credentials are currently hardcoded in `appsettings.json` for demonstration purposes only
- The `appsettings.json` file contains sample values intended for local development and testing

**Production Security Implementation:**
In a production environment, all sensitive credentials would be managed securely:
- **Azure Key Vault**: All secrets (API keys, database passwords, connection strings) are stored in Azure Key Vault
- **CI/CD Pipeline**: GitHub Actions securely retrieves secrets from Key Vault during deployment
- **Environment Variables**: Pods in Kubernetes receive secrets injected as environment variables from Kubernetes Secrets
- **No Hardcoded Credentials**: Production images never contain sensitive data
- **Secret Rotation**: Regular rotation of API keys and database passwords implemented via scheduled jobs

Refer to [DEPLOYMENT_STRATEGY.md](DEPLOYMENT_STRATEGY.md) for detailed production security architecture including Azure Key Vault integration, secret management, and CI/CD pipeline configuration.

## Logging

- **Request Logging Middleware**: Captures request metadata, HTTP method, path, response status codes, elapsed time, and unhandled exceptions
- **Distributed Tracing Support**: Integrates with Activity.Current to capture trace IDs, operation IDs, and span IDs for observability
- Logs are configured in `appsettings.json` under `Logging:LogLevel`

## Run the solution

1. Configure the SQL Server connection string in `src/Flash.SensitiveWords.API/appsettings.json` under `ConnectionStrings:DefaultConnection`.
2. Optionally update the API key in `appsettings.json` under `ApiSettings:ApiKey` for production use.
3. Run the solution or start the API project.
4. The API will initialize the database and seed default sensitive words if needed.

## Swagger

- Swagger is configured in `src/Flash.SensitiveWords.API/Program.cs`
- In development, open the Swagger UI from the API host URL to inspect endpoints, request/response contracts, and required security headers
- XML documentation comments are included from the assembly for detailed endpoint descriptions
- Security scheme for API Key authentication is documented in the Swagger UI
## Interview Questions

- **Performance Enhancements:** See `INTERVIEW_Q1_Performance_Enhancements.md` for caching, filtering optimization, database tuning, batch operations, compression, and performance testing strategy.
- **Additional Project Enhancements:** See `INTERVIEW_Q2_Additional_Enhancements.md` for audit logging, soft deletes, categorization, validation, rate limiting, health checks, API versioning, import/export, analytics, webhooks, and more.
- **Production Deployment:** See `DEPLOYMENT_STRATEGY.md` for a production deployment walkthrough using Azure, containers, Kubernetes, CI/CD, database migration, secret management, and observability.
## Middleware

- **ApiKeyMiddleware**: Validates `X-Api-Key` header for CRUD endpoints, allowing the filter endpoint to remain public
- **RequestLoggingMiddleware**: Logs HTTP requests with performance metrics and exception tracking using structured logging with trace IDs

## Notes

- The MVC frontend is intentionally decoupled from the database; it interacts with the API through `Flash.SensitiveWords.RestClient`
- The architecture is designed for maintainability and easy extension across service, domain, and infrastructure boundaries
- **Security Note**: The credentials in this repository (API keys, connection strings) are sample values for demonstration only. Production deployments implement enterprise-grade secret management using Azure Key Vault with automated retrieval and rotation policies
- For production deployment guidance, see [DEPLOYMENT_STRATEGY.md](DEPLOYMENT_STRATEGY.md)
