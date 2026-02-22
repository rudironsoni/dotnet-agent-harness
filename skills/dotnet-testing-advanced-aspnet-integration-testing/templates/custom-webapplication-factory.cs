// =============================================================================
// ASP.NET Core Integration Testing - Custom WebApplicationFactory Template
// =============================================================================
// Purpose: Create test-specific application factory, configure in-memory database and test services
// Usage: Inherit from this class or modify to fit project requirements
// =============================================================================

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace YourProject.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory for integration testing
/// </summary>
/// <typeparam name="TProgram">Application entry point class</typeparam>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // ========================================
            // 1. Remove original database configuration
            // ========================================
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Can also use RemoveAll to remove all related services at once
            // services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            // ========================================
            // 2. Add in-memory database
            // ========================================
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDatabase");
            });

            // ========================================
            // 3. Replace external services with test versions
            // ========================================
            // Email service
            services.Replace(ServiceDescriptor.Scoped<IEmailService, TestEmailService>());
            
            // External API service
            services.Replace(ServiceDescriptor.Scoped<IExternalApiService, MockExternalApiService>());
            
            // File service
            services.Replace(ServiceDescriptor.Scoped<IFileService, InMemoryFileService>());

            // ========================================
            // 4. Initialize database
            // ========================================
            var serviceProvider = services.BuildServiceProvider();
            
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Ensure database is created
            context.Database.EnsureCreated();
            
            // Optional: Add seed data
            // SeedTestData(context);
        });

        // ========================================
        // 5. Configure test environment
        // ========================================
        builder.UseEnvironment("Testing");

        // ========================================
        // 6. Override configuration values
        // ========================================
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("Logging:LogLevel:Default", "Warning"),
                new KeyValuePair<string, string?>("ConnectionStrings:TestDb", "InMemory"),
            });
        });
    }

    /// <summary>
    /// Add test seed data
    /// </summary>
    private static void SeedTestData(AppDbContext context)
    {
        // Example: Add test shipper data
        if (!context.Shippers.Any())
        {
            context.Shippers.AddRange(
                new Shipper
                {
                    CompanyName = "Test Logistics A",
                    Phone = "02-12345678",
                    CreatedAt = DateTime.UtcNow
                },
                new Shipper
                {
                    CompanyName = "Test Logistics B",
                    Phone = "02-87654321",
                    CreatedAt = DateTime.UtcNow
                }
            );
            context.SaveChanges();
        }
    }
}

// =============================================================================
// Test service implementation examples
// =============================================================================

/// <summary>
/// Test email service - does not actually send emails
/// </summary>
public class TestEmailService : IEmailService
{
    public List<(string To, string Subject, string Body)> SentEmails { get; } = new();

    public Task SendEmailAsync(string to, string subject, string body)
    {
        // Record email content without actually sending
        SentEmails.Add((to, subject, body));
        return Task.CompletedTask;
    }
}

/// <summary>
/// Test external API service - returns mock data
/// </summary>
public class MockExternalApiService : IExternalApiService
{
    public Task<string> GetDataAsync()
    {
        return Task.FromResult("Mock API Response");
    }
}

/// <summary>
/// Test file service - uses in-memory storage
/// </summary>
public class InMemoryFileService : IFileService
{
    private readonly Dictionary<string, byte[]> _files = new();

    public Task SaveFileAsync(string path, byte[] content)
    {
        _files[path] = content;
        return Task.CompletedTask;
    }

    public Task<byte[]?> GetFileAsync(string path)
    {
        return Task.FromResult(_files.GetValueOrDefault(path));
    }

    public Task<bool> FileExistsAsync(string path)
    {
        return Task.FromResult(_files.ContainsKey(path));
    }
}
