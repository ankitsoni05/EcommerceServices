# Clean Architecture - Practical Examples & Scenarios

## Table of Contents
1. [Clean Architecture Fundamentals](#clean-architecture-fundamentals)
   - [What is Clean Architecture?](#what-is-clean-architecture)
   - [Why is Clean Architecture Used?](#why-is-clean-architecture-used)
   - [When to Use Clean Architecture?](#when-to-use-clean-architecture)
   - [Core Principles & Dependency Flow](#core-principles--dependency-flow)
   - [Layer Purposes in Detail](#layer-purposes-in-detail)
   - [EF Core & DbContext Placement](#ef-core--dbcontext-placement)
   - [Repository Pattern in Clean Architecture](#repository-pattern-in-clean-architecture)
   - [Benefits & Trade-Offs](#benefits--trade-offs)
2. [Why Each File Exists](#why-each-file-exists)
3. [Real-World Scenarios](#real-world-scenarios)
4. [Common Modifications](#common-modifications)
5. [Step-by-Step Walkthroughs](#step-by-step-walkthroughs)
6. [Troubleshooting Guide](#troubleshooting-guide)

---

## Clean Architecture Fundamentals

### What is Clean Architecture?

**Clean Architecture** is a software design philosophy created by **Robert C. Martin (Uncle Bob)** that emphasizes:
1. **Separation of concerns** - Different parts of the application have different responsibilities
2. **Independence** - Core business logic is independent of frameworks, databases, and UI
3. **Testability** - Easy to test without external dependencies
4. **Maintainability** - Changes in one area don't ripple through the entire system

#### The Core Idea (Simple Explanation)

Imagine you're running a restaurant:
- Your **recipes** (business rules) shouldn't change based on whether customers order via phone, app, or in-person
- Your **kitchen processes** shouldn't depend on what brand of refrigerator you use
- If you replace your refrigerator, you don't need to rewrite your recipes

**Clean Architecture applies this same principle to software:**
```
Your business logic (the "recipes") should be:
? Independent of the database
? Independent of the UI/API
? Independent of frameworks
? Independent of external services
```

#### The Architectural Layers

Clean Architecture organizes code into concentric circles (like an onion):

```
???????????????????????????????????????????????????????????
?  PRESENTATION (Controllers, UI, API)                    ?
?  ????????????????????????????????????????????????????? ?
?  ?  APPLICATION (Services, Use Cases, Business Logic)? ?
?  ?  ??????????????????????????????????????????????? ? ?
?  ?  ?  DOMAIN (Entities, Business Rules)          ? ? ?
?  ?  ?        ? THE CORE ?                       ? ? ?
?  ?  ?  (No dependencies on outer layers)          ? ? ?
?  ?  ??????????????????????????????????????????????? ? ?
?  ????????????????????????????????????????????????????? ?
?  INFRASTRUCTURE (Database, External Services)          ?
???????????????????????????????????????????????????????????
```

**Key Point:** Dependencies point **INWARD ONLY**. The inner circles don't know about the outer circles.

---

### Why is Clean Architecture Used?

Let's understand through **real-world problems** it solves:

#### Problem 1: Tightly Coupled Code (The Bad Way)

```csharp
// ? BAD: Everything mixed together
public class ProductController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
    {
        // Database code directly in controller
        var connection = new SqlConnection("Server=localhost;...");
        var command = new SqlCommand("SELECT * FROM Products", connection);
        connection.Open();
        var reader = command.ExecuteReader();
        
        // Business logic mixed in
        var products = new List<Product>();
        while (reader.Read())
        {
            // Calculating discount (business rule)
            var price = (decimal)reader["Price"];
            var discount = price > 1000 ? 0.15m : 0.10m;
            var finalPrice = price * (1 - discount);
            
            products.Add(new Product 
            { 
                Name = (string)reader["Name"],
                Price = finalPrice 
            });
        }
        
        connection.Close();
        return Ok(products);
    }
}
```

**Problems with this approach:**
1. ? **Can't test** without a real database
2. ? **Can't reuse** business logic in mobile app or different API
3. ? **Hard to change** database (SQL Server ? MongoDB requires rewriting everything)
4. ? **Violates Single Responsibility** - Controller does everything
5. ? **Business rules scattered** - Discount logic buried in controller
6. ? **No flexibility** - Everything is hardcoded

#### Solution: Clean Architecture (The Good Way)

```csharp
// ? GOOD: Separated concerns

// 1. DOMAIN: Business entity (pure business concept)
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    
    public decimal CalculateDiscountedPrice()
    {
        var discount = Price > 1000 ? 0.15m : 0.10m;
        return Price * (1 - discount);
    }
}

// 2. DOMAIN: Repository contract (what operations we need)
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
}

// 3. APPLICATION: Business logic service
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    
    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<IEnumerable<ProductDto>> GetDiscountedProductsAsync()
    {
        var products = await _repository.GetAllAsync();
        return products.Select(p => new ProductDto
        {
            Name = p.Name,
            OriginalPrice = p.Price,
            DiscountedPrice = p.CalculateDiscountedPrice()
        });
    }
}

// 4. INFRASTRUCTURE: Database implementation (how to get data)
public class SqlProductRepository : IProductRepository
{
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }
}

// 5. PRESENTATION: Controller (HTTP concerns only)
public class ProductController : ControllerBase
{
    private readonly IProductService _service;
    
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _service.GetDiscountedProductsAsync();
        return Ok(products);
    }
}
```

**Benefits of Clean Architecture:**
1. ? **Easy to test** - Mock the repository, no database needed
2. ? **Reusable** - Same service works for Web API, Mobile API, Desktop
3. ? **Flexible** - Swap SQL with MongoDB by changing one registration
4. ? **Clear responsibilities** - Each class has one job
5. ? **Business rules protected** - Discount logic in Domain, safe and reusable
6. ? **Maintainable** - Changes are isolated

---

### When to Use Clean Architecture?

#### ? Use Clean Architecture When:

1. **Long-term projects** (will be maintained for years)
   - Example: Enterprise applications, SaaS products
   
2. **Team projects** (multiple developers)
   - Clear boundaries help teams work independently
   
3. **Business logic is complex**
   - Example: E-commerce (pricing, inventory, discounts, shipping)
   - Example: Banking (transactions, interest calculations, compliance)
   
4. **Requirements change frequently**
   - Easy to modify without breaking everything
   
5. **Multiple platforms/clients**
   - Same business logic for Web, Mobile, Desktop
   
6. **Automated testing is important**
   - Architecture makes testing straightforward
   
7. **Technology might change**
   - Example: Starting with PostgreSQL, might move to MongoDB
   - Example: Starting with REST API, might add GraphQL

**Real-world example:**
```
E-commerce Application:
- Web storefront (customers)
- Mobile app (customers)
- Admin panel (staff)
- API for partners
? Same product catalog logic for ALL platforms
```

#### ? Don't Use Clean Architecture When:

1. **Proof of concept** or **prototype**
   - Too much structure slows you down
   
2. **Throwaway projects**
   - Not meant to be maintained
   
3. **Very simple CRUD apps**
   - Example: Basic TODO list with 3 endpoints
   - Overhead > Benefits
   
4. **Learning projects / tutorials**
   - Unless you're specifically learning Clean Architecture
   
5. **Tight deadlines with simple requirements**
   - Example: "Build a simple contact form by Friday"
   
6. **Solo weekend projects**
   - Keep it simple, ship fast

**Rule of thumb:**
```
If your app is:
- < 5 endpoints + simple logic ? Skip Clean Architecture
- 5-20 endpoints + moderate logic ? Consider Clean Architecture
- > 20 endpoints + complex logic ? Definitely use Clean Architecture
```

---

### Core Principles & Dependency Flow

#### The Dependency Rule (The Most Important Rule)

> **"Source code dependencies must point only INWARD, toward higher-level policies."**
> — Robert C. Martin

**What this means in simple terms:**
```
Inner layers (Domain) can NEVER depend on outer layers (Controllers, Database)
Outer layers CAN depend on inner layers

Think: The core of your business doesn't care about technical details
```

#### Visual Representation

```
????????????????????????????????????????????
?  Presentation (Controllers, UI)          ? ? Depends on Application
????????????????????????????????????????????
?  Application (Services, Use Cases)       ? ? Depends on Domain
????????????????????????????????????????????
?  Domain (Entities, Business Rules)       ? ? Depends on NOTHING
????????????????????????????????????????????
?  Infrastructure (Database, External)     ? ? Implements Domain interfaces
????????????????????????????????????????????

Arrows show: "depends on" or "knows about"
```

#### Correct Dependency Flow

```
? ALLOWED (Outer ? Inner):

Controller       ?  Service          (OK)
Service          ?  Entity           (OK)
Service          ?  IRepository      (OK)
Repository       ?  IRepository      (OK - implements interface)
Repository       ?  Entity           (OK)
Controller       ?  IService         (OK)
```

```
? FORBIDDEN (Inner ? Outer):

Entity           ?  Service          (NEVER!)
Entity           ?  Repository       (NEVER!)
Entity           ?  Controller       (NEVER!)
Domain           ?  Infrastructure   (NEVER!)
Service          ?  Controller       (NEVER!)
```

#### Why This Rule Exists

**Analogy: Building a House**
```
Foundation (Domain)
  ? supports
Walls (Application)
  ? supports
Roof (Presentation)

The foundation doesn't know about the roof.
The roof depends on walls, walls depend on foundation.
You can change the roof color without touching the foundation.
```

**In Software:**
```
Domain (Foundation)
  ? used by
Application (Walls)
  ? used by
Presentation (Roof)

Change UI? Domain stays the same.
Change database? Domain stays the same.
Domain is STABLE and PROTECTED.
```

#### Dependency Inversion Principle

**The Problem:**
```csharp
// Application layer wants to use database
public class ProductService
{
    private SqlProductRepository _repo = new SqlProductRepository(); // ? Direct dependency on SQL
    
    public Product GetProduct(int id)
    {
        return _repo.GetById(id);
    }
}
```
**Issue:** Service now depends on specific database implementation. Can't swap databases, hard to test.

**The Solution: Use Interfaces**
```csharp
// 1. Domain defines WHAT it needs (interface)
public interface IProductRepository
{
    Product GetById(int id);
}

// 2. Application uses the interface
public class ProductService
{
    private readonly IProductRepository _repo; // ? Depends on abstraction
    
    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }
    
    public Product GetProduct(int id)
    {
        return _repo.GetById(id);
    }
}

// 3. Infrastructure implements the interface
public class SqlProductRepository : IProductRepository
{
    public Product GetById(int id)
    {
        // SQL implementation
    }
}

public class MongoProductRepository : IProductRepository
{
    public Product GetById(int id)
    {
        // MongoDB implementation
    }
}

// 4. Program.cs wires it up
builder.Services.AddScoped<IProductRepository, SqlProductRepository>();
// OR
builder.Services.AddScoped<IProductRepository, MongoProductRepository>();
// Service code doesn't change! ?

```

### EF Core & DbContext Placement

#### Where Does DbContext Go?
DbContext belongs in the Infrastructure layer (e.g., `Infrastructure/Data/CatalogDbContext.cs`)

#### Why Not Domain?
- Domain must remain persistence-agnostic.
- EF Core is an implementation detail (ORM specifics, mapping APIs, fluent configuration).
- Keeping Domain pure allows swapping EF Core for Dapper, MongoDB, or an in-memory implementation without touching business rules.

#### Why Not Application?
- Application coordinates use cases; it should depend on abstractions (repository interfaces) not concrete persistence mechanics.
- Putting DbContext in Application couples use cases to persistence technology, violating the Dependency Rule.

#### Why Infrastructure Is Correct
- Infrastructure is the boundary where external technical concerns live: databases, file systems, message brokers, email, cache.
- DbContext depends on Domain entities (allowed: outward ? inward dependency direction). The Domain does not depend back on DbContext.

#### Example Structure
```
Domain/
  Entities/ (CatalogItem, CatalogBrand, CatalogType)
  Interfaces/ (ICatalogRepository)
Application/
  Services/ (CatalogService)
  DTOs/
Infrastructure/
  Data/ (CatalogDbContext)
  Repositories/ (EfCoreCatalogRepository)
Presentation/
  Controllers/ (CatalogController)
```

#### Repository Flow with EF Core
1. Controller calls Service (`ICatalogService`).
2. Service calls Repository via `ICatalogRepository` abstraction.
3. DI container provides concrete `EfCoreCatalogRepository`.
4. Repository uses `CatalogDbContext` (Infrastructure) to execute queries.
5. Domain entities are materialized and returned upward.

#### DbContext Responsibilities
- Manage unit-of-work and change tracking.
- Translate LINQ queries to SQL.
- Coordinate transactions (implicitly or explicitly).
- Apply entity configuration (fluent API / IEntityTypeConfiguration classes).

#### What Stays Out of DbContext
- Business workflows (belong in Application).
- Domain invariants beyond basic structural constraints (complex invariants remain inside entities or domain services).
- Presentation concerns (no HTTP, serialization logic).

### Benefits & Trade-Offs

#### Benefits of Proper Layering
- Testability: Mocks for `ICatalogRepository` avoid hitting database.
- Replaceability: Swap `EfCoreCatalogRepository` with `InMemoryCatalogRepository` or `SqlCatalogRepository` with one line in DI registration.
- Maintainability: Changes to database schema affect only Infrastructure.
- Domain Integrity: Business rules remain untouched by persistence concerns.
- Reduced Coupling: Minimizes ripple effect when adopting new storage.

#### Performance Considerations
- Infrastructure layer can introduce caching decorators without altering Domain/Application.
- Query optimization lives in repositories; adjustments do not contaminate business logic.

#### Common Mistakes
- Putting `[Table]`, `[Column]`, or EF-specific attributes in Domain entities (couples Domain to EF Core).
- Injecting `DbContext` directly into Controllers (skips Application orchestration, bloats presentation logic).
- Performing business calculations inside repository methods (mixes concerns; keep them in Domain/Application).

#### Trade-Offs
| Aspect | Clean Architecture | Flat Architecture |
|--------|--------------------|-------------------|
| Initial Setup | Higher | Lower |
| Long-Term Flexibility | High | Low |
| Test Friendliness | High | Medium/Low |
| Cognitive Load (Small App) | Higher | Lower |
| Separation of Concerns | Clear | Blurred |

#### When EF Core Mapping Needs More Detail
Use `IEntityTypeConfiguration<T>` classes inside Infrastructure:
```csharp
// Infrastructure/Data/Configurations/CatalogItemConfiguration.cs
public class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        builder.ToTable("CatalogItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.HasOne(x => x.CatalogBrand)
               .WithMany()
               .HasForeignKey(x => x.CatalogBrandId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CatalogType)
               .WithMany()
               .HasForeignKey(x => x.CatalogTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
```
And apply automatically:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
}
```

#### Dependency Movement Summary
```
Presentation ? Application ? Domain ? Infrastructure (implements)
```
- Arrows point toward stability (Domain).
- Infrastructure depends on Domain entities & interfaces — never the reverse.
- Application depends on Domain abstractions — never on Infrastructure concretes.
- Presentation depends on Application — never on Domain internals directly (other than DTOs returned through services).

#### Checklist: Adding EF Core Correctly
- [ ] Create DbContext in Infrastructure.
- [ ] Keep Domain entities POCO (no EF attributes unless unavoidable).* 
- [ ] Implement repository using DbContext.
- [ ] Register DbContext & repository in DI in Program.cs.
- [ ] Keep queries optimized inside repository.
- [ ] Return Domain entities to Application layer; convert to DTOs before leaving Application.

*If using attributes (e.g., `[Key]`) for speed in a small project, treat it as a pragmatic compromise, but prefer fluent configs for purity.

---

## Why Each File Exists

<!-- existing content continues below -->
