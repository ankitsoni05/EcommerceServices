# Quick Start Guide - EF Core + PostgreSQL

## ?? Quick Setup (5 Minutes)

### 1. Install PostgreSQL with Docker
```bash
docker run --name postgres-catalog \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  -d postgres:16
```

### 2. Update Password (if needed)
Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "CatalogDb": "Host=localhost;Port=5432;Database=CatalogDb_Dev;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### 3. Create Migration
```bash
cd CatalogService
dotnet ef migrations add InitialCreate
```

### 4. Run Application
```bash
dotnet run
```

Database will be created automatically!

---

## ?? EF Core Commands Cheat Sheet

### Migrations
```bash
# Add new migration
dotnet ef migrations add <MigrationName>

# Apply migrations
dotnet ef database update

# Rollback to specific migration
dotnet ef database update <MigrationName>

# Remove last migration (not applied yet)
dotnet ef migrations remove

# List all migrations
dotnet ef migrations list

# Generate SQL script
dotnet ef migrations script

# Drop database
dotnet ef database drop
```

### DbContext
```bash
# Scaffold from existing database
dotnet ef dbcontext scaffold "Host=localhost;Database=mydb;Username=postgres;Password=pass" Npgsql.EntityFrameworkCore.PostgreSQL

# List DbContexts
dotnet ef dbcontext list

# Generate SQL for DbContext
dotnet ef dbcontext script
```

---

## ?? Switch Between Implementations

### Use In-Memory (Development/Testing)
```csharp
// Program.cs
builder.Services.AddScoped<ICatalogRepository, InMemoryCatalogRepository>();
// Comment out DbContext registration
```

### Use PostgreSQL (Production)
```csharp
// Program.cs
builder.Services.AddScoped<ICatalogRepository, EfCoreCatalogRepository>();
// Keep DbContext registration
```

---

## ?? Test the API

### Get all items
```bash
curl http://localhost:5000/api/catalog
```

### Get item by ID
```bash
curl http://localhost:5000/api/catalog/1
```

### Create new item
```bash
curl -X POST http://localhost:5000/api/catalog \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New Product",
    "description": "Description",
    "price": 99.99,
    "availableStock": 10,
    "imageUrl": "/images/product.jpg",
    "catalogBrandId": 1,
    "catalogTypeId": 1
  }'
```

### Update item
```bash
curl -X PUT http://localhost:5000/api/catalog/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated Product",
    "description": "Updated Description",
    "price": 199.99,
    "availableStock": 20,
    "imageUrl": "/images/updated.jpg",
    "catalogBrandId": 1,
    "catalogTypeId": 1
  }'
```

### Delete item
```bash
curl -X DELETE http://localhost:5000/api/catalog/1
```

---

## ??? PostgreSQL Quick Commands

### Connect to database
```bash
# Using psql
psql -U postgres -d CatalogDb_Dev

# Using Docker
docker exec -it postgres-catalog psql -U postgres -d CatalogDb_Dev
```

### Common queries
```sql
-- List tables
\dt

-- Describe table
\d "CatalogItems"

-- View data
SELECT * FROM "CatalogItems";
SELECT * FROM "CatalogBrands";
SELECT * FROM "CatalogTypes";

-- Count records
SELECT COUNT(*) FROM "CatalogItems";

-- Query with join
SELECT 
    ci."Name" as Product,
    ci."Price",
    cb."Name" as Brand,
    ct."Name" as Type
FROM "CatalogItems" ci
JOIN "CatalogBrands" cb ON ci."CatalogBrandId" = cb."Id"
JOIN "CatalogTypes" ct ON ci."CatalogTypeId" = ct."Id";
```

---

## ?? Troubleshooting

### Problem: "Could not connect to PostgreSQL"
```bash
# Check if PostgreSQL is running
docker ps

# If not running, start it
docker start postgres-catalog

# Check connection string in appsettings.Development.json
```

### Problem: "Migration already applied"
```bash
# Check migration status
dotnet ef migrations list

# If needed, rollback
dotnet ef database update <PreviousMigration>

# Then apply again
dotnet ef database update
```

### Problem: "Table already exists"
```bash
# Drop database and recreate
dotnet ef database drop
dotnet ef database update
```

### Problem: "Build failed"
```bash
# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

---

## ?? Project Structure

```
CatalogService/
??? Domain/                    ? Pure business logic (no changes)
?   ??? Entities/
?   ??? Interfaces/
??? Application/               ? Use cases (no changes)
?   ??? Services/
?   ??? Interfaces/
?   ??? DTOs/
??? Infrastructure/            ? NEW - All EF Core code
?   ??? Data/
?   ?   ??? CatalogDbContext.cs
?   ?   ??? Configurations/
?   ??? Repositories/
?       ??? InMemoryCatalogRepository.cs
?       ??? EfCoreCatalogRepository.cs  ? NEW
??? Controllers/               ? API endpoints (no changes)
??? Program.cs                 ? UPDATED - DI registration
??? appsettings.json          ? UPDATED - Connection string
```

---

## ?? What Changed vs What Stayed the Same

### ? NO Changes (Clean Architecture Power!)
- ? Domain entities (still POCO)
- ? Domain interfaces
- ? Application services
- ? Application DTOs
- ? Controllers
- ? Business logic

### ? Changes (Only Infrastructure)
- ? Added DbContext
- ? Added Entity Configurations
- ? Added EF Core Repository
- ? Updated Program.cs DI registration
- ? Added connection string

**Switch implementation:** Change **ONE line** in Program.cs! ??

---

## ?? Key Files to Know

| File | Purpose | Location |
|------|---------|----------|
| `CatalogDbContext.cs` | Database connection | `Infrastructure/Data/` |
| `CatalogItemConfiguration.cs` | Table schema | `Infrastructure/Data/Configurations/` |
| `EfCoreCatalogRepository.cs` | Data access | `Infrastructure/Repositories/` |
| `Program.cs` | DI registration | Root |
| `appsettings.json` | Connection string | Root |

---

## ?? Learning Resources

- **Full documentation:** See `IMPLEMENTATION_GUIDE.md`
- **Clean Architecture:** See `CLEAN_ARCHITECTURE_GUIDE.md`
- **Practical examples:** See `PRACTICAL_EXAMPLES.md`
- **EF Core docs:** https://learn.microsoft.com/en-us/ef/core/

---

*Ready to go! Start coding! ??*
