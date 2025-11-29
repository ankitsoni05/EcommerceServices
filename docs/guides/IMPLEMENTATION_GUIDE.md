# EF Core + PostgreSQL Implementation Guide

## ?? What We Implemented

We've successfully implemented **Entity Framework Core with PostgreSQL** following Clean Architecture principles and Repository Pattern.

---

## ?? File Structure Created

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
??? Infrastructure/                   (NEW - All EF Core code here)
?   ??? Data/
?   ?   ??? CatalogDbContext.cs                    ? DbContext
?   ?   ??? Configurations/
?   ?       ??? CatalogItemConfiguration.cs        ? Entity mapping
?   ?       ??? CatalogBrandConfiguration.cs       ? Entity mapping
?   ?       ??? CatalogTypeConfiguration.cs        ? Entity mapping
?   ??? Repositories/
?       ??? InMemoryCatalogRepository.cs           ? Original (kept)
?       ??? EfCoreCatalogRepository.cs             ? NEW EF Core impl
?
??? Program.cs                        (UPDATED - DI registration)
??? appsettings.json                  (UPDATED - Connection string)
??? appsettings.Development.json      (UPDATED - Dev connection string)
```

---

## ?? Why Each File Exists & Where It Goes

### 1. **CatalogDbContext.cs** ? `Infrastructure/Data/`

**What it is:** EF Core DbContext - the bridge between your app and PostgreSQL.

**Why Infrastructure:**
- ? It's a technical detail (EF Core framework)
- ? Domain entities stay pure (no EF attributes)
- ? Can be replaced with Dapper/MongoDB without changing Domain
- ? Follows Dependency Rule (Infrastructure ? Domain)

**Responsibilities:**
```csharp
public class CatalogDbContext : DbContext
{
    // 1. Define DbSets (tables)
    public DbSet<CatalogItem> CatalogItems { get; set; }
    
    // 2. Apply entity configurations
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
    
    // 3. Seed data (optional)
}
```

**Key Features:**
- Unit of Work pattern (tracks changes, commits atomically)
- Change tracking for entities
- LINQ to SQL translation
- Transaction management

---

### 2. **Entity Configurations** ? `Infrastructure/Data/Configurations/`

**What they are:** Fluent API configurations for mapping entities to database tables.

**Why separate files:**
- ? Keeps DbContext clean and focused
- ? One file per entity (easy to find and maintain)
- ? Domain entities stay POCO (no attributes)

**Example - CatalogItemConfiguration.cs:**
```csharp
public class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        // Table name
        builder.ToTable("CatalogItems");
        
        // Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.Price)
            .HasPrecision(18, 2);
        
        // Relationships
        builder.HasOne(x => x.CatalogBrand)
            .WithMany()
            .HasForeignKey(x => x.CatalogBrandId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes for performance
        builder.HasIndex(x => x.Name);
    }
}
```

**Benefits:**
- Database schema is explicit and documented
- No magic [Column], [Table] attributes in Domain
- Easy to modify without touching entities

---

### 3. **EfCoreCatalogRepository.cs** ? `Infrastructure/Repositories/`

**What it is:** EF Core implementation of `ICatalogRepository` interface.

**Repository Pattern in Action:**
```csharp
// Interface defined in Domain
public interface ICatalogRepository
{
    Task<IEnumerable<CatalogItem>> GetAllItemsAsync();
    Task<CatalogItem?> GetItemByIdAsync(int id);
    // ...
}

// Implementation in Infrastructure
public class EfCoreCatalogRepository : ICatalogRepository
{
    private readonly CatalogDbContext _context;
    
    public async Task<IEnumerable<CatalogItem>> GetAllItemsAsync()
    {
        return await _context.CatalogItems
            .Include(i => i.CatalogBrand)
            .Include(i => i.CatalogType)
            .AsNoTracking()
            .ToListAsync();
    }
}
```

**Key Patterns Used:**

#### A. **Eager Loading** (Avoiding N+1 Problem)
```csharp
// ? BAD - N+1 queries
var items = await _context.CatalogItems.ToListAsync();
// For each item, EF makes another query for Brand and Type!

// ? GOOD - Single query with JOINs
var items = await _context.CatalogItems
    .Include(i => i.CatalogBrand)
    .Include(i => i.CatalogType)
    .ToListAsync();
```

#### B. **AsNoTracking()** (Read-Only Queries)
```csharp
// For read-only queries (no updates)
var items = await _context.CatalogItems
    .AsNoTracking() // ? Better performance, less memory
    .ToListAsync();

// For updates (needs tracking)
var item = await _context.CatalogItems.FindAsync(id);
item.Price = 999.99m;
await _context.SaveChangesAsync(); // EF detects changes
```

#### C. **Add/Update/Delete Pattern**
```csharp
// Add
_context.CatalogItems.Add(item);
await _context.SaveChangesAsync();

// Update
_context.CatalogItems.Update(item);
await _context.SaveChangesAsync();

// Delete
var item = await _context.CatalogItems.FindAsync(id);
_context.CatalogItems.Remove(item);
await _context.SaveChangesAsync();
```

---

### 4. **Program.cs** ? Root

**What changed:** Added DbContext registration and repository switching.

```csharp
// ========================================
// DATABASE CONFIGURATION
// ========================================
builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("CatalogDb"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5));
        });
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ========================================
// REPOSITORY PATTERN - SWITCH IMPLEMENTATIONS
// ========================================
// Development/Testing
// builder.Services.AddScoped<ICatalogRepository, InMemoryCatalogRepository>();

// Production with PostgreSQL
builder.Services.AddScoped<ICatalogRepository, EfCoreCatalogRepository>();

// ? This is the MAGIC of Clean Architecture!
// Change ONE line, everything else stays the same!
```

**Auto-Migration in Development:**
```csharp
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await dbContext.Database.MigrateAsync();
}
```

---

### 5. **appsettings.json** ? Root

**Connection Strings:**

```json
// Production
{
  "ConnectionStrings": {
    "CatalogDb": "Host=localhost;Port=5432;Database=CatalogDb;Username=postgres;Password=your_password"
  }
}

// Development
{
  "ConnectionStrings": {
    "CatalogDb": "Host=localhost;Port=5432;Database=CatalogDb_Dev;Username=postgres;Password=postgres;Include Error Detail=true"
  }
}
```

**Security Best Practices:**
- ? Use User Secrets for local development
- ? Use Environment Variables for production
- ? Use Azure Key Vault for cloud deployments
- ? Never commit passwords to source control

---

## ?? How to Run

### Step 1: Install PostgreSQL

**Option A: Docker (Recommended)**
```bash
docker run --name postgres-catalog -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16
```

**Option B: Download PostgreSQL**
- Download from: https://www.postgresql.org/download/
- Install and set password during setup

### Step 2: Update Connection String

Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "CatalogDb": "Host=localhost;Port=5432;Database=CatalogDb_Dev;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### Step 3: Create Migration

```bash
# Navigate to project directory
cd CatalogService

# Add migration
dotnet ef migrations add InitialCreate

# This creates:
# - Migrations/YYYYMMDDHHMMSS_InitialCreate.cs (migration code)
# - Migrations/CatalogDbContextModelSnapshot.cs (model snapshot)
```

### Step 4: Apply Migration

```bash
# Apply to database
dotnet ef database update

# Or just run the app (auto-migration enabled in Program.cs)
dotnet run
```

### Step 5: Verify Database

```sql
-- Connect to PostgreSQL
psql -U postgres -d CatalogDb_Dev

-- List tables
\dt

-- View data
SELECT * FROM "CatalogItems";
SELECT * FROM "CatalogBrands";
SELECT * FROM "CatalogTypes";
```

---

## ?? Clean Architecture Benefits Demonstrated

### Before (Without Clean Architecture)
```csharp
// ? Controller directly uses DbContext
public class CatalogController : ControllerBase
{
    private readonly CatalogDbContext _context;
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await _context.CatalogItems
            .Include(i => i.Brand)
            .Where(i => i.Price > 100)
            .ToListAsync();
        return Ok(items);
    }
}

// Problems:
// - Can't test without database
// - Can't reuse logic
// - Hard to change database
// - SQL in controller (wrong place)
```

### After (With Clean Architecture)
```csharp
// ? Controller uses service (Application layer)
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _service;
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await _service.GetAllItemsAsync();
        return Ok(items);
    }
}

// Benefits:
// ? Easy to test (mock ICatalogService)
// ? Logic reusable in mobile app
// ? Can swap database by changing DI registration
// ? SQL in repository (right place)
```

---

## ?? Switching Implementations (The Power of Repository Pattern)

### Scenario 1: Development with In-Memory Data
```csharp
// Program.cs
builder.Services.AddScoped<ICatalogRepository, InMemoryCatalogRepository>();

// No database needed!
// Fast startup
// Predictable test data
```

### Scenario 2: Production with PostgreSQL
```csharp
// Program.cs
builder.Services.AddScoped<ICatalogRepository, EfCoreCatalogRepository>();

// Persistent data
// Full SQL features
// Production-ready
```

### Scenario 3: Testing with Mock
```csharp
// Unit test
var mockRepo = new Mock<ICatalogRepository>();
mockRepo.Setup(r => r.GetAllItemsAsync())
    .ReturnsAsync(new List<CatalogItem> { /* test data */ });

var service = new CatalogService(mockRepo.Object);
var result = await service.GetAllItemsAsync();

// No database needed!
// Fast tests
// Controlled data
```

**What DOESN'T change:**
- ? Domain entities
- ? Application services
- ? Controllers
- ? DTOs
- ? Business logic

**What DOES change:**
- ? ONE line in Program.cs

---

## ?? Database Schema Created

### Tables

#### CatalogBrands
```sql
CREATE TABLE "CatalogBrands" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL UNIQUE
);
```

#### CatalogTypes
```sql
CREATE TABLE "CatalogTypes" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL UNIQUE
);
```

#### CatalogItems
```sql
CREATE TABLE "CatalogItems" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500),
    "Price" DECIMAL(18,2) NOT NULL,
    "AvailableStock" INTEGER NOT NULL,
    "ImageUrl" VARCHAR(500),
    "CatalogBrandId" INTEGER NOT NULL,
    "CatalogTypeId" INTEGER NOT NULL,
    CONSTRAINT "FK_CatalogItems_Brands" FOREIGN KEY ("CatalogBrandId") 
        REFERENCES "CatalogBrands"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CatalogItems_Types" FOREIGN KEY ("CatalogTypeId") 
        REFERENCES "CatalogTypes"("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_CatalogItems_Name" ON "CatalogItems"("Name");
CREATE INDEX "IX_CatalogItems_CatalogBrandId" ON "CatalogItems"("CatalogBrandId");
CREATE INDEX "IX_CatalogItems_CatalogTypeId" ON "CatalogItems"("CatalogTypeId");
CREATE INDEX "IX_CatalogItems_Price" ON "CatalogItems"("Price");
```

### Seed Data
- 5 Brands (Samsung, Apple, Sony, Microsoft, Dell)
- 5 Types (Smartphone, Laptop, Headphones, Tablet, Monitor)
- 5 Sample Products

---

## ?? Testing the Implementation

### Test 1: Get All Items
```bash
curl http://localhost:5000/api/catalog
```

**Expected Response:**
```json
[
  {
    "id": 1,
    "name": "Samsung Galaxy S24",
    "price": 999.99,
    "brandName": "Samsung",
    "typeName": "Smartphone"
  },
  // ...
]
```

### Test 2: Get Item by ID
```bash
curl http://localhost:5000/api/catalog/1
```

### Test 3: Create New Item
```bash
curl -X POST http://localhost:5000/api/catalog \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Surface Laptop 5",
    "description": "Microsoft premium laptop",
    "price": 1299.99,
    "availableStock": 20,
    "imageUrl": "/images/surface-laptop-5.jpg",
    "catalogBrandId": 4,
    "catalogTypeId": 2
  }'
```

### Test 4: Check Database
```sql
-- Connect to PostgreSQL
psql -U postgres -d CatalogDb_Dev

-- Verify the new item
SELECT * FROM "CatalogItems" WHERE "Name" = 'Surface Laptop 5';
```

---

## ?? Key Concepts Explained

### 1. **Repository Pattern**
- Interface in Domain (`ICatalogRepository`)
- Implementation in Infrastructure (`EfCoreCatalogRepository`)
- Application uses interface (doesn't know about EF Core)

### 2. **Unit of Work** (Built into EF Core)
```csharp
// DbContext IS a Unit of Work
var item1 = new CatalogItem { /* ... */ };
var item2 = new CatalogItem { /* ... */ };

_context.CatalogItems.Add(item1);
_context.CatalogItems.Add(item2);

await _context.SaveChangesAsync(); // ? Atomic transaction
```

### 3. **Dependency Injection**
```csharp
// Registration (Program.cs)
builder.Services.AddScoped<ICatalogRepository, EfCoreCatalogRepository>();

// Injection (Controller/Service)
public CatalogService(ICatalogRepository repository)
{
    _repository = repository; // DI magic!
}
```

### 4. **Eager Loading vs Lazy Loading**
```csharp
// Eager Loading (1 query) ?
var items = await _context.CatalogItems
    .Include(i => i.CatalogBrand)
    .Include(i => i.CatalogType)
    .ToListAsync();

// Lazy Loading (N+1 queries) ?
var items = await _context.CatalogItems.ToListAsync();
// Each access to item.CatalogBrand triggers a new query!
```

---

## ??? Common EF Core Patterns

### Pattern 1: Find vs FirstOrDefault vs Single

```csharp
// Find - Uses primary key, checks cache first
var item = await _context.CatalogItems.FindAsync(id);

// FirstOrDefault - Flexible query, always hits database
var item = await _context.CatalogItems
    .FirstOrDefaultAsync(i => i.Name == "iPhone");

// Single - Throws if 0 or >1 results
var item = await _context.CatalogItems
    .SingleAsync(i => i.Id == id);
```

### Pattern 2: Tracking vs No-Tracking

```csharp
// Tracking (for updates) - EF monitors changes
var item = await _context.CatalogItems.FindAsync(id);
item.Price = 999.99m;
await _context.SaveChangesAsync(); // EF knows it changed

// No-Tracking (read-only) - Better performance
var items = await _context.CatalogItems
    .AsNoTracking()
    .ToListAsync();
// Can't update these items
```

### Pattern 3: Projection (Select specific fields)

```csharp
// Get all data ?
var items = await _context.CatalogItems
    .Include(i => i.CatalogBrand)
    .ToListAsync();

// Get only needed data ?
var items = await _context.CatalogItems
    .Select(i => new
    {
        i.Name,
        i.Price,
        BrandName = i.CatalogBrand.Name
    })
    .ToListAsync();
```

---

## ?? Next Steps

1. **Add Caching** - Implement `CachedCatalogRepository` decorator
2. **Add Specifications** - For complex queries
3. **Add Unit Tests** - Test services with mock repositories
4. **Add Integration Tests** - Test with real database
5. **Add Pagination** - For large datasets
6. **Add Filtering** - Dynamic query building
7. **Add Monitoring** - Log query performance

---

## ?? Summary

### What We Built
? EF Core with PostgreSQL integration
? Repository Pattern implementation
? Clean Architecture maintained
? Seed data for testing
? Auto-migration in development
? Comprehensive documentation

### Architecture Benefits Demonstrated
? Domain stays pure (no EF attributes)
? Application stays clean (no SQL)
? Infrastructure is swappable
? Easy to test (mock repositories)
? One-line implementation switching

### Clean Architecture Layers
```
Presentation ? Application ? Domain ? Infrastructure
     ?              ?           ?           ?
Controllers ? Services ? Interfaces ? Repositories
                              ?
                         Entities (POCO)
```

**The POWER:** Change database technology without touching business logic! ??

---

*Implementation complete! Ready for production use.* ?
