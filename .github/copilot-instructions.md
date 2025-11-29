# E-Commerce Microservices - AI Coding Instructions

## Architecture Overview

This is a **Clean Architecture** microservices system with strict layer separation. The architecture enforces dependency rules: Domain has no dependencies, Application depends only on Domain, Infrastructure depends on Domain/Application, and Presentation depends on all layers.

### Layer Responsibilities (CatalogService Example)

- **Domain** (`Domain/`): Pure business entities (e.g., `CatalogItem.cs`), repository interfaces (`ICatalogRepository`). No framework dependencies.
- **Application** (`Application/`): Business logic services (`CatalogService.cs`), DTOs (`CatalogItemDto`), service interfaces (`ICatalogService`). Framework-agnostic.
- **Infrastructure** (`Infrastructure/`): EF Core implementations (`CatalogDbContext`, `EfCoreCatalogRepository`), database configurations, external integrations.
- **Presentation** (`Controllers/`): HTTP concerns, API endpoints. Controllers inject `ICatalogService`, never repositories directly.

### Critical Dependency Flow
```
Controllers → ICatalogService → CatalogService → ICatalogRepository → EfCoreCatalogRepository → DbContext
```

**Never** make Domain or Application depend on Infrastructure (e.g., don't reference EF Core or DbContext in Domain/Application layers).

## Key Architectural Patterns

### Repository Pattern
- Interfaces defined in `Domain/Interfaces/` (e.g., `ICatalogRepository`)
- Implementations in `Infrastructure/Repositories/` (e.g., `EfCoreCatalogRepository`, `InMemoryCatalogRepository`)
- Swap implementations via DI in `Program.cs`: `builder.Services.AddScoped<ICatalogRepository, EfCoreCatalogRepository>()`
- Repositories return Domain entities, NOT DTOs

### Service Layer Pattern
- Application services handle business logic and DTO mapping
- Example: `CatalogService.GetAllItemsAsync()` calls repository, maps entities to DTOs
- Services throw domain exceptions (e.g., `KeyNotFoundException`), controllers catch and return HTTP status

### Entity Configuration
- EF Core configurations live in `Infrastructure/Data/Configurations/` using `IEntityTypeConfiguration<T>`
- Apply all configurations via `modelBuilder.ApplyConfigurationsFromAssembly()` in `DbContext.OnModelCreating()`
- Example: `CatalogItemConfiguration` defines table schema, relationships, indexes

## Development Workflows

### Docker-First Development (Recommended)
```bash
# Start all services with databases
docker-compose up -d

# View service logs
docker-compose logs -f catalog-service

# Rebuild after code changes
docker-compose up --build -d catalog-service

# Stop everything
docker-compose down
```

### Database Migrations (EF Core)
```bash
# Navigate to service project
cd CatalogService

# Create migration (from project root or service folder)
dotnet ef migrations add MigrationName

# Apply migrations (automatic in Development via Program.cs)
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove
```

**Important**: Migrations run automatically in Development mode via `Program.cs` using `dbContext.Database.MigrateAsync()`.

### Local Development (Without Docker)
Requires PostgreSQL on port 5432. Connection string in `appsettings.Development.json`:
```json
"ConnectionStrings": {
  "CatalogDb": "Host=localhost;Database=CatalogDb;Username=postgres;Password=postgres"
}
```

Run: `dotnet run` from service directory.

## Project Conventions

### Naming & Structure
- Services: `{Domain}Service` (e.g., `CatalogService`, not `CatalogServiceImpl`)
- Repositories: `EfCore{Domain}Repository` or `InMemory{Domain}Repository`
- DTOs: `{Entity}Dto` for reads, `Create{Entity}Dto` for writes
- Controllers: `{Domain}Controller` with `[Route("api/[controller]")]`

### Dependency Injection (Program.cs)
```csharp
// DbContext with retry logic for transient failures
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), null)));

// Repository (swap implementations here)
builder.Services.AddScoped<ICatalogRepository, EfCoreCatalogRepository>();

// Application service
builder.Services.AddScoped<ICatalogService, CatalogService>();
```

### EF Core Query Patterns
- Always use `.Include()` for related entities to avoid N+1 queries
- Use `.AsNoTracking()` for read-only queries (performance)
- Example: `_context.CatalogItems.Include(i => i.CatalogBrand).Include(i => i.CatalogType).AsNoTracking()`

### Data Seeding
- Seed data defined in `DbContext.OnModelCreating()` using `modelBuilder.Entity<T>().HasData()`
- Includes sample brands, types, and catalog items for development

## Service Communication

### Docker Networking
- All services communicate via `ecommerce-network` bridge network
- Service names resolve as hostnames (e.g., `postgres:5432`, `redis:6379`)
- PostgreSQL creates multiple databases via init script: `CatalogDb`, `OrderDb`, `IdentityDb`

### Connection Strings in Docker
Use service names, not `localhost`:
```json
"CatalogDb": "Host=postgres;Database=CatalogDb;Username=postgres;Password=postgres_dev_password"
```

## Testing Approach

### In-Memory Testing
Use `InMemoryCatalogRepository` for unit tests by swapping DI registration:
```csharp
builder.Services.AddScoped<ICatalogRepository, InMemoryCatalogRepository>();
```

### Integration Testing
Start dependencies via Docker Compose, test against real databases.

## Common Mistakes to Avoid

1. **Don't inject `DbContext` into Application services** - use repositories
2. **Don't return Domain entities from controllers** - always map to DTOs
3. **Don't put business logic in repositories** - they only handle data access
4. **Don't use `localhost` in Docker connection strings** - use service names
5. **Don't forget `.Include()` for related entities** - results in N+1 queries
6. **Don't apply migrations manually in Development** - handled automatically in `Program.cs`

## Documentation Structure

Comprehensive docs in `docs/`:
- `docs/architecture/CLEAN_ARCHITECTURE_GUIDE.md` - Theory and layer explanations
- `docs/docker/DOCKER_QUICK_START.md` - 3-minute Docker setup
- `docs/guides/MIGRATIONS_GUIDE.md` - EF Core migration workflows
- `docs/guides/IMPLEMENTATION_GUIDE.md` - Step-by-step feature implementation

Always reference existing docs when explaining architecture or workflows.

## Adding New Features

### Example: Add New Entity
1. Create entity in `Domain/Entities/` (POCO, no attributes)
2. Add `DbSet<NewEntity>` to `CatalogDbContext`
3. Create configuration in `Infrastructure/Data/Configurations/`
4. Add repository methods to `ICatalogRepository` and implement in `EfCoreCatalogRepository`
5. Create DTOs in `Application/DTOs/`
6. Implement service methods in `Application/Services/`
7. Add controller endpoints in `Controllers/`
8. Run `dotnet ef migrations add AddNewEntity`

### Example: Add New Microservice
1. Copy `CatalogService/` structure
2. Add service to `docker-compose.yml` with unique port
3. Add database to PostgreSQL init script if needed
4. Update `ecommerce-network` configuration

## Environment & Configuration

- **Development**: Auto-migration, sensitive data logging enabled
- **Production**: Manual migrations, minimal logging
- Override files: `docker-compose.override.yml` (dev), `docker-compose.prod.yml` (prod)
- Environment variables via `.env` file (not committed, use `.env.example` template)
