using CatalogService.Application.Interfaces;
using CatalogService.Domain.Interfaces;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ========================================
// DATABASE CONFIGURATION (Infrastructure)
// ========================================
// Add DbContext with PostgreSQL
// Connection string is stored in appsettings.json for security
builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("CatalogDb"),
        npgsqlOptions =>
        {
            // Retry on transient failures (network issues, timeouts)
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            
            // Set command timeout (optional)
            npgsqlOptions.CommandTimeout(30);
        });
    
    // Enable sensitive data logging in development only
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ========================================
// REDIS CACHE CONFIGURATION
// ========================================
// Add Redis distributed cache for high-performance caching
// Benefits:
// - Shared cache across multiple instances (horizontal scaling)
// - Persistent cache (survives application restarts)
// - Fast in-memory data store
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "CatalogService:"; // Prefix for all cache keys
});

// ========================================
// REPOSITORY PATTERN REGISTRATION
// ========================================
// Switch between implementations by changing ONE line:

// Option 1: In-Memory Repository (for testing/development)
// builder.Services.AddScoped<ICatalogRepository, InMemoryCatalogRepository>();

// Option 2: EF Core Repository with PostgreSQL (for production)
builder.Services.AddScoped<ICatalogRepository, EfCoreCatalogRepository>();

// This is the POWER of Clean Architecture + Repository Pattern:
// - Business logic (CatalogService) doesn't change
// - Controllers don't change
// - Only this registration line changes
// - Easy to test with different implementations

// ========================================
// APPLICATION SERVICES (with Decorator Pattern for Caching)
// ========================================
// Register the core service
builder.Services.AddScoped<ICatalogService, CatalogService.Application.Services.CatalogService>();

// Decorate with caching layer using Decorator pattern
// This wraps the original service with caching behavior
// Benefits:
// - No modification to original CatalogService
// - Caching can be toggled on/off easily
// - Follows Open/Closed Principle (SOLID)
builder.Services.Decorate<ICatalogService, CatalogService.Application.Services.CachedCatalogService>();

// Alternative manual registration (if Scrutor not available):
// builder.Services.AddScoped<ICatalogService>(provider =>
// {
//     var innerService = new CatalogService.Application.Services.CatalogService(
//         provider.GetRequiredService<ICatalogRepository>());
//     var cache = provider.GetRequiredService<IDistributedCache>();
//     var logger = provider.GetRequiredService<ILogger<CachedCatalogService>>();
//     return new CachedCatalogService(innerService, cache, logger);
// });

var app = builder.Build();

// ========================================
// DATABASE INITIALIZATION (Development)
// ========================================
// Automatically apply migrations in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    
    try
    {
        // Apply pending migrations
        await dbContext.Database.MigrateAsync();
        app.Logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error applying database migrations");
        // In production, you might want to throw here
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
