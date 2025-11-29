# Catalog Service - Clean Architecture

This service follows Clean Architecture principles with clear separation of concerns across multiple layers.

---

## ?? Documentation Has Moved!

**All comprehensive documentation has been organized in the main `docs/` folder.**

### ?? [**View Complete Documentation Index**](../docs/INDEX.md)

---

## ?? Quick Links

### Getting Started
- **[Docker Quick Start](../docs/docker/DOCKER_QUICK_START.md)** - Get running in 3 minutes
- **[Quick Start Guide](../docs/guides/QUICKSTART.md)** - Local development setup

### Architecture
- **[Clean Architecture Guide](../docs/architecture/CLEAN_ARCHITECTURE_GUIDE.md)** - Theory and principles
- **[Architecture Diagrams](../docs/architecture/ARCHITECTURE_DIAGRAMS.md)** - Visual flow charts
- **[Practical Examples](../docs/architecture/PRACTICAL_EXAMPLES.md)** - Real-world scenarios

### Implementation
- **[Implementation Guide](../docs/guides/IMPLEMENTATION_GUIDE.md)** - EF Core + PostgreSQL
- **[Migrations Guide](../docs/guides/MIGRATIONS_GUIDE.md)** - Database versioning

### Docker
- **[Docker Compose Guide](../docs/docker/DOCKER_COMPOSE_GUIDE.md)** - Complete orchestration
- **[Docker Architecture](../docs/docker/DOCKER_ARCHITECTURE_DIAGRAMS.md)** - System diagrams

---

## ?? Architecture Overview

### Current Implementation: EF Core + PostgreSQL + Repository Pattern

```
???????????????????????????????????????????????????????????
?  PRESENTATION (Controllers, API)                        ?
?  ? CatalogController                                   ?
???????????????????????????????????????????????????????????
                       ? depends on
                       ?
???????????????????????????????????????????????????????????
?  APPLICATION (Services, Use Cases, DTOs)                ?
?  ? CatalogService                                      ?
?  ? CatalogItemDto, CreateCatalogItemDto                ?
???????????????????????????????????????????????????????????
                       ? depends on
                       ?
???????????????????????????????????????????????????????????
?  DOMAIN (Entities, Business Rules, Interfaces)         ?
?  ? CatalogItem, CatalogBrand, CatalogType             ?
?  ? ICatalogRepository (interface)                     ?
???????????????????????????????????????????????????????????
                       ? implements
???????????????????????????????????????????????????????????
?  INFRASTRUCTURE (Database, External Services)          ?
?  ? CatalogDbContext (EF Core)                         ?
?  ? Entity Configurations                              ?
?  ? EfCoreCatalogRepository (PostgreSQL)               ?
?  ? InMemoryCatalogRepository (Testing)                ?
???????????????????????????????????????????????????????????
```

---

## ?? Project Structure

```
CatalogService/
??? Domain/                           (No changes - stays pure)
?   ??? Entities/
?   ?   ??? CatalogItem.cs
?   ?   ??? CatalogBrand.cs
?   ?   ??? CatalogType.cs
?   ??? Interfaces/
?       ??? ICatalogRepository.cs
?
??? Application/                      (No changes - stays pure)
?   ??? Services/
?   ?   ??? CatalogService.cs
?   ??? Interfaces/
?   ?   ??? ICatalogService.cs
?   ??? DTOs/
?
??? Infrastructure/                   (All EF Core code here)
?   ??? Data/
?   ?   ??? CatalogDbContext.cs
?   ?   ??? Configurations/
?   ??? Repositories/
?       ??? InMemoryCatalogRepository.cs
?       ??? EfCoreCatalogRepository.cs
?
??? Controllers/
?   ??? CatalogController.cs
?
??? Program.cs
??? Dockerfile
??? CatalogService.csproj
```

---

## ?? API Endpoints

### Catalog Items

- `GET /api/catalog` - Get all catalog items
- `GET /api/catalog/{id}` - Get item by ID
- `GET /api/catalog/brand/{brandId}` - Get items by brand
- `GET /api/catalog/type/{typeId}` - Get items by type
- `POST /api/catalog` - Create new catalog item
- `PUT /api/catalog/{id}` - Update existing item
- `DELETE /api/catalog/{id}` - Delete item

---

## ?? Quick Start

### Option 1: With Docker (Recommended)

```bash
# From solution root
docker-compose up -d
curl http://localhost:5001/api/catalog
```

**?? [Detailed Docker guide ?](../docs/docker/DOCKER_QUICK_START.md)**

### Option 2: Local Development

```bash
# Start PostgreSQL
docker run --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16

# Run migrations
cd CatalogService
dotnet ef database update

# Run application
dotnet run
```

**?? [Detailed local guide ?](../docs/guides/QUICKSTART.md)**

---

## ?? Switching Between Implementations

### Development (In-Memory)
```csharp
// Program.cs
builder.Services.AddScoped<ICatalogRepository, InMemoryCatalogRepository>();
```

### Production (PostgreSQL)
```csharp
// Program.cs
builder.Services.AddScoped<ICatalogRepository, EfCoreCatalogRepository>();
```

**That's it!** One line change. No changes to:
- Domain entities
- Application services
- Controllers
- DTOs
- Business logic

**?? [Learn more about Repository Pattern ?](../docs/architecture/PRACTICAL_EXAMPLES.md#repository-pattern-in-clean-architecture)**

---

## ??? Technology Stack

- **.NET 10** - Latest .NET version
- **ASP.NET Core** - Web framework
- **Entity Framework Core 10** - ORM
- **PostgreSQL** - Database
- **Npgsql** - PostgreSQL provider for EF Core
- **Docker** - Containerization

---

## ?? Testing

### Unit Tests (Mock Repository)
```csharp
var mockRepo = new Mock<ICatalogRepository>();
mockRepo.Setup(r => r.GetAllItemsAsync())
    .ReturnsAsync(testData);

var service = new CatalogService(mockRepo.Object);
var result = await service.GetAllItemsAsync();
```

**?? [Testing guide ?](../docs/guides/IMPLEMENTATION_GUIDE.md#testing-the-implementation)**

---

## ?? Database Schema

### Tables
- **CatalogItems** - Product catalog
- **CatalogBrands** - Product brands
- **CatalogTypes** - Product categories

### Seed Data
- 5 Brands (Samsung, Apple, Sony, Microsoft, Dell)
- 5 Types (Smartphone, Laptop, Headphones, Tablet, Monitor)
- 5 Sample Products

**?? [Database schema details ?](../docs/guides/IMPLEMENTATION_GUIDE.md#database-schema-created)**

---

## ?? Learning Resources

### Start Here
1. [Clean Architecture Guide](../docs/architecture/CLEAN_ARCHITECTURE_GUIDE.md) - Understand why
2. [Architecture Diagrams](../docs/architecture/ARCHITECTURE_DIAGRAMS.md) - Visualize structure
3. [Practical Examples](../docs/architecture/PRACTICAL_EXAMPLES.md) - See patterns in action

### Deep Dive
4. [Implementation Guide](../docs/guides/IMPLEMENTATION_GUIDE.md) - Build features
5. [Migrations Guide](../docs/guides/MIGRATIONS_GUIDE.md) - Manage database
6. [Docker Guide](../docs/docker/DOCKER_COMPOSE_GUIDE.md) - Deploy services

---

## ?? Benefits of This Architecture

1. **Separation of Concerns** - Each layer has a clear responsibility
2. **Testability** - Easy to unit test each layer independently
3. **Maintainability** - Changes in one layer don't affect others
4. **Flexibility** - Easy to swap implementations
5. **Domain-Centric** - Business logic is independent
6. **Repository Pattern** - Clean abstraction over data access
7. **Production Ready** - EF Core with PostgreSQL

---

## ?? Complete Documentation

**All comprehensive guides are now in the main docs folder:**

?? **[View Complete Documentation Index](../docs/INDEX.md)**

---

*For questions or issues, refer to the troubleshooting sections in the documentation.*
