using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database - uses Session lifetime, auto-cleanup after tests
var postgres = builder.AddPostgres("postgres")
                     .WithLifetime(ContainerLifetime.Session);

var postgresDb = postgres.AddDatabase("productdb");

// Add Redis cache - uses Session lifetime, auto-cleanup after tests
var redis = builder.AddRedis("redis")
                   .WithLifetime(ContainerLifetime.Session);

// Add API service - uses strongly-typed project reference
// Aspire automatically handles service discovery and connection string injection
var apiProject = builder.AddProject<Projects.MyApp_Api>("myapp-api")
                        .WithReference(postgresDb)
                        .WithReference(redis);

// Notes:
// 1. Don't manually configure WithHttpEndpoint(), let Aspire handle it
// 2. ContainerLifetime.Session ensures containers auto-cleanup after tests
// 3. Use WithReference() to establish service dependencies

builder.Build().Run();
