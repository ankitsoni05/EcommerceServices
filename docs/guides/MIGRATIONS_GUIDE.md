# Migration Workflow & Database Management

## ?? Table of Contents
1. [Understanding Migrations](#understanding-migrations)
2. [Creating Your First Migration](#creating-your-first-migration)
3. [Migration Commands](#migration-commands)
4. [Common Scenarios](#common-scenarios)
5. [Best Practices](#best-practices)
6. [Troubleshooting](#troubleshooting)

---

## Understanding Migrations

### What are Migrations?

**Migrations** are version control for your database schema. They track changes to your entity models and generate SQL to update the database.

**Think of it like Git for your database:**
- Each migration is like a commit
- You can see history of changes
- You can rollback to previous versions
- You can review changes before applying

### Migration Files Structure

When you run `dotnet ef migrations add InitialCreate`, EF Core creates:

```
Migrations/
??? 20240115120000_InitialCreate.cs           ? Up/Down methods
??? 20240115120000_InitialCreate.Designer.cs  ? Metadata
??? CatalogDbContextModelSnapshot.cs          ? Current model state
```

**Example Migration File:**
```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Forward migration - creates tables
        migrationBuilder.CreateTable(
            name: "CatalogBrands",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", 
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CatalogBrands", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverse migration - drops tables
        migrationBuilder.DropTable(name: "CatalogBrands");
    }
}
```

---

## Creating Your First Migration

### Step 1: Install EF Core Tools

```bash
# Global tool (recommended)
dotnet tool install --global dotnet-ef

# Verify installation
dotnet ef --version
```

### Step 2: Navigate to Project

```bash
cd CatalogService
```

### Step 3: Create Migration

```bash
dotnet ef migrations add InitialCreate

# Output:
# Build started...
# Build succeeded.
# Done. To undo this action, use 'ef migrations remove'
```

**What happens:**
1. EF Core scans your DbContext and entities
2. Compares with previous snapshot (or creates first one)
3. Generates migration code (Up/Down methods)
4. Creates migration files in `Migrations/` folder

### Step 4: Review Migration

Open `Migrations/YYYYMMDDHHMMSS_InitialCreate.cs` and review:
- Tables being created
- Columns and their types
- Indexes and constraints
- Foreign keys

### Step 5: Apply Migration

```bash
# Apply to database
dotnet ef database update

# Output:
# Build started...
# Build succeeded.
# Applying migration '20240115120000_InitialCreate'.
# Done.
```

**What happens:**
1. EF Core connects to database
2. Creates `__EFMigrationsHistory` table (tracks applied migrations)
3. Runs the `Up()` method
4. Records migration in history table

### Step 6: Verify in Database

```bash
# Connect to PostgreSQL
psql -U postgres -d CatalogDb_Dev

# List tables
\dt

# Output:
# __EFMigrationsHistory
# CatalogBrands
# CatalogItems
# CatalogTypes

# Check migration history
SELECT * FROM "__EFMigrationsHistory";
```

---

## Migration Commands

### Create Migration

```bash
# Basic syntax
dotnet ef migrations add <MigrationName>

# Examples
dotnet ef migrations add InitialCreate
dotnet ef migrations add AddProductDescription
dotnet ef migrations add AddIndexOnProductName
```

**Naming conventions:**
- Use PascalCase
- Be descriptive
- Indicate what changed
- Good: `AddProductDescription`, `RemoveObsoleteColumns`
- Bad: `Update1`, `Fix`, `Changes`

### Apply Migrations

```bash
# Apply all pending migrations
dotnet ef database update

# Apply to specific migration
dotnet ef database update InitialCreate

# Generate SQL script (don't apply)
dotnet ef migrations script

# Generate script for specific range
dotnet ef migrations script AddProductDescription AddIndexOnProductName
```

### Remove Migration

```bash
# Remove last migration (not applied yet)
dotnet ef migrations remove

# Force remove (even if applied - dangerous!)
dotnet ef migrations remove --force
```

### List Migrations

```bash
# List all migrations
dotnet ef migrations list

# Output shows:
# 20240115120000_InitialCreate (Applied)
# 20240116130000_AddProductDescription (Pending)
```

### Rollback Migration

```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Rollback all migrations (empty database)
dotnet ef database update 0
```

### Drop Database

```bash
# Delete entire database
dotnet ef database drop

# With confirmation prompt
dotnet ef database drop --force
```

---

## Common Scenarios

### Scenario 1: Adding a New Column

**1. Update Entity:**
```csharp
// Domain/Entities/CatalogItem.cs
public class CatalogItem
{
    // ...existing properties
    public string Barcode { get; set; } = string.Empty; // NEW
}
```

**2. Create Migration:**
```bash
dotnet ef migrations add AddBarcodeColumn
```

**3. Review Generated Code:**
```csharp
public partial class AddBarcodeColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Barcode",
            table: "CatalogItems",
            type: "text",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Barcode",
            table: "CatalogItems");
    }
}
```

**4. Apply Migration:**
```bash
dotnet ef database update
```

---

### Scenario 2: Adding an Index

**1. Update Configuration:**
```csharp
// Infrastructure/Data/Configurations/CatalogItemConfiguration.cs
public void Configure(EntityTypeBuilder<CatalogItem> builder)
{
    // ...existing configuration
    
    // NEW INDEX
    builder.HasIndex(x => x.Barcode)
        .IsUnique();
}
```

**2. Create Migration:**
```bash
dotnet ef migrations add AddBarcodeIndex
```

**3. Generated Code:**
```csharp
public partial class AddBarcodeIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_CatalogItems_Barcode",
            table: "CatalogItems",
            column: "Barcode",
            unique: true);
    }
}
```

---

### Scenario 3: Renaming a Column

**1. Update Entity:**
```csharp
public class CatalogItem
{
    public string ProductName { get; set; } = string.Empty; // Was "Name"
}
```

**2. Create Migration with RenameColumn:**
```bash
dotnet ef migrations add RenameNameToProductName
```

**3. Manually Edit Migration (EF might not detect rename):**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.RenameColumn(
        name: "Name",
        table: "CatalogItems",
        newName: "ProductName");
}
```

---

### Scenario 4: Adding a New Table

**1. Create New Entity:**
```csharp
// Domain/Entities/ProductReview.cs
public class ProductReview
{
    public int Id { get; set; }
    public int CatalogItemId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; }
}
```

**2. Add DbSet to Context:**
```csharp
public class CatalogDbContext : DbContext
{
    public DbSet<ProductReview> ProductReviews { get; set; } = null!;
}
```

**3. Create Configuration:**
```csharp
public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("ProductReviews");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Rating).IsRequired();
        // ...
    }
}
```

**4. Create Migration:**
```bash
dotnet ef migrations add AddProductReviews
```

---

### Scenario 5: Seeding Data

**1. Add Seed Data in Migration:**
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.InsertData(
        table: "CatalogBrands",
        columns: new[] { "Id", "Name" },
        values: new object[,]
        {
            { 1, "Samsung" },
            { 2, "Apple" },
            { 3, "Sony" }
        });
}
```

**2. Or Use HasData in Configuration:**
```csharp
public void Configure(EntityTypeBuilder<CatalogBrand> builder)
{
    builder.HasData(
        new CatalogBrand { Id = 1, Name = "Samsung" },
        new CatalogBrand { Id = 2, Name = "Apple" },
        new CatalogBrand { Id = 3, Name = "Sony" }
    );
}
```

---

## Best Practices

### ? DO:

1. **Create descriptive migration names**
```bash
? dotnet ef migrations add AddProductCategories
? dotnet ef migrations add UpdatePriceColumnPrecision
? dotnet ef migrations add Update1
```

2. **Review migrations before applying**
```bash
# Review generated code
cat Migrations/20240115120000_InitialCreate.cs

# Generate SQL to review
dotnet ef migrations script > migration.sql
```

3. **Keep migrations small and focused**
```bash
# Good
dotnet ef migrations add AddProductDescription
dotnet ef migrations add AddProductBarcode

# Bad (too many changes)
dotnet ef migrations add UpdateEverything
```

4. **Test migrations in development first**
```bash
# Test in dev environment
dotnet ef database update --connection "Host=localhost;Database=CatalogDb_Test"

# Then apply to production
```

5. **Use migrations for data seeding**
```csharp
// Good - In migration or HasData
builder.HasData(new CatalogBrand { Id = 1, Name = "Samsung" });

// Bad - In application code at startup
// _context.CatalogBrands.Add(new CatalogBrand { Name = "Samsung" });
```

### ? DON'T:

1. **Don't modify applied migrations**
```bash
? Never edit migration that's been applied to production
? Create a new migration to fix issues
```

2. **Don't commit sensitive data**
```csharp
? builder.HasData(new User { Password = "admin123" });
? Use separate seeding scripts with secure data
```

3. **Don't skip migration history**
```bash
? Deleting __EFMigrationsHistory table
? Use proper rollback commands
```

---

## Troubleshooting

### Problem: "Migration already applied"

```bash
# Check migration status
dotnet ef migrations list

# If migration shows as (Applied), you need to:
# Option 1: Create a new migration
dotnet ef migrations add FixIssue

# Option 2: Rollback and reapply (development only!)
dotnet ef database update PreviousMigration
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Problem: "Build failed"

```bash
# Ensure project builds first
dotnet build

# Check for compilation errors
# Fix errors, then retry migration command
```

### Problem: "Could not connect to database"

```bash
# Check PostgreSQL is running
docker ps | grep postgres

# Check connection string
cat appsettings.Development.json

# Test connection
psql -U postgres -h localhost -p 5432
```

### Problem: "Foreign key constraint violation"

```csharp
// Ensure seed data references are correct
builder.HasData(
    new CatalogItem 
    { 
        Id = 1, 
        CatalogBrandId = 1, // Must exist in CatalogBrands
        CatalogTypeId = 1   // Must exist in CatalogTypes
    }
);
```

### Problem: "Column type mismatch"

```csharp
// Update configuration to match database
builder.Property(x => x.Price)
    .HasColumnType("decimal(18,2)"); // Explicit type

// Then create migration
dotnet ef migrations add FixPriceColumnType
```

---

## Production Deployment Workflow

### Option 1: Auto-Migration (Simple Apps)

```csharp
// Program.cs
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await dbContext.Database.MigrateAsync();
}
```

**Pros:** Simple, automatic
**Cons:** Risky for production, no rollback plan

### Option 2: SQL Script (Recommended)

```bash
# Generate SQL script
dotnet ef migrations script --output migration.sql

# Review script
cat migration.sql

# Apply manually to production database
psql -U postgres -d CatalogDb_Prod -f migration.sql
```

**Pros:** Full control, can review before applying
**Cons:** Manual process

### Option 3: CI/CD Pipeline

```yaml
# Azure DevOps / GitHub Actions
- name: Generate migration script
  run: dotnet ef migrations script --idempotent --output $(Build.ArtifactStagingDirectory)/migration.sql

- name: Deploy to database
  run: psql -f migration.sql
```

---

## Migration History Table

EF Core tracks applied migrations in `__EFMigrationsHistory`:

```sql
SELECT * FROM "__EFMigrationsHistory";

-- Output:
-- MigrationId                    | ProductVersion
-- ------------------------------|---------------
-- 20240115120000_InitialCreate  | 8.0.0
-- 20240116130000_AddBarcode     | 8.0.0
```

**This table:**
- Tracks which migrations have been applied
- Used by EF Core to determine pending migrations
- Should never be manually modified
- Automatically managed by `dotnet ef database update`

---

## Quick Reference

| Task | Command |
|------|---------|
| Create migration | `dotnet ef migrations add <Name>` |
| Apply migrations | `dotnet ef database update` |
| Rollback to migration | `dotnet ef database update <Name>` |
| Remove last migration | `dotnet ef migrations remove` |
| List migrations | `dotnet ef migrations list` |
| Generate SQL script | `dotnet ef migrations script` |
| Drop database | `dotnet ef database drop` |

---

*Complete migration workflow documented! Ready for production! ??*
