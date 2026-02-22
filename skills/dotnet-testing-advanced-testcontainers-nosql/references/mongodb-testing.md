# MongoDB Containerized Testing

> This document is extracted from [SKILL.md](../SKILL.md) and contains complete code examples and detailed explanations for MongoDB containerized testing.

---

### MongoDB Container Fixture

Using Collection Fixture pattern to share containers, saving over 80% of test time:

```csharp
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace YourProject.Integration.Tests.Fixtures;

/// <summary>
/// MongoDB Container Fixture - Implements IAsyncLifetime to manage container lifecycle
/// </summary>
public class MongoDbContainerFixture : IAsyncLifetime
{
    private MongoDbContainer? _container;

    public IMongoDatabase Database { get; private set; } = null!;
    public string ConnectionString { get; private set; } = string.Empty;
    public string DatabaseName { get; } = "testdb";

    public async Task InitializeAsync()
    {
        // Use MongoDB 7.0 to ensure feature completeness
        _container = new MongoDbBuilder()
                     .WithImage("mongo:7.0")
                     .WithPortBinding(27017, true)
                     .Build();

        await _container.StartAsync();

        ConnectionString = _container.GetConnectionString();
        var client = new MongoClient(ConnectionString);
        Database = client.GetDatabase(DatabaseName);
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// Clear all collections in database - used for test isolation
    /// </summary>
    public async Task ClearDatabaseAsync()
    {
        var collections = await Database.ListCollectionNamesAsync();
        await collections.ForEachAsync(async collectionName =>
        {
            await Database.DropCollectionAsync(collectionName);
        });
    }
}

/// <summary>
/// Define test collection using MongoDB Fixture
/// </summary>
[CollectionDefinition("MongoDb Collection")]
public class MongoDbCollectionFixture : ICollectionFixture<MongoDbContainerFixture>
{
    // This class doesn't need implementation, just for marking collection
}
```

### MongoDB Document Model Design

Create document models with complex structures including nested objects, arrays, dictionaries:

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YourProject.Core.Models.Mongo;

/// <summary>
/// User Document - Demonstrates MongoDB complex document structure
/// </summary>
public class UserDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("username")]
    [BsonRequired]
    public string Username { get; set; } = string.Empty;

    [BsonElement("email")]
    [BsonRequired]
    public string Email { get; set; } = string.Empty;

    [BsonElement("profile")]
    public UserProfile Profile { get; set; } = new();

    [BsonElement("addresses")]
    public List<Address> Addresses { get; set; } = new();

    [BsonElement("skills")]
    public List<Skill> Skills { get; set; } = new();

    [BsonElement("preferences")]
    public Dictionary<string, object> Preferences { get; set; } = new();

    [BsonElement("created_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updated_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("is_active")]
    public bool IsActive { get; set; } = true;

    [BsonElement("version")]
    public int Version { get; set; } = 1;

    /// <summary>
    /// Optimistic locking version increment
    /// </summary>
    public void IncrementVersion(DateTime updateTime)
    {
        Version++;
        UpdatedAt = updateTime;
    }
}

/// <summary>
/// User Profile - Nested document example
/// </summary>
public class UserProfile
{
    [BsonElement("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("last_name")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("birth_date")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? BirthDate { get; set; }

    [BsonElement("bio")]
    public string Bio { get; set; } = string.Empty;

    [BsonElement("social_links")]
    public Dictionary<string, string> SocialLinks { get; set; } = new();

    [BsonIgnore]
    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// Address Model - For geospatial queries
/// </summary>
public class Address
{
    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // "home", "work", "other"

    [BsonElement("city")]
    public string City { get; set; } = string.Empty;

    [BsonElement("country")]
    public string Country { get; set; } = string.Empty;

    [BsonElement("location")]
    public GeoLocation? Location { get; set; }

    [BsonElement("is_primary")]
    public bool IsPrimary { get; set; }
}

/// <summary>
/// Geo Location - GeoJSON format
/// </summary>
public class GeoLocation
{
    [BsonElement("type")]
    public string Type { get; set; } = "Point";

    [BsonElement("coordinates")]
    public double[] Coordinates { get; set; } = new double[2]; // [longitude, latitude]

    public static GeoLocation CreatePoint(double longitude, double latitude)
    {
        return new GeoLocation
        {
            Type = "Point",
            Coordinates = new[] { longitude, latitude }
        };
    }
}

/// <summary>
/// Skill Model - Array query example
/// </summary>
public class Skill
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("level")]
    public SkillLevel Level { get; set; } = SkillLevel.Beginner;

    [BsonElement("years_experience")]
    public int YearsExperience { get; set; }

    [BsonElement("verified")]
    public bool Verified { get; set; }
}

/// <summary>
/// Skill Level Enumeration
/// </summary>
public enum SkillLevel
{
    [BsonRepresentation(BsonType.String)]
    Beginner,

    [BsonRepresentation(BsonType.String)]
    Intermediate,

    [BsonRepresentation(BsonType.String)]
    Advanced,

    [BsonRepresentation(BsonType.String)]
    Expert
}
```

### BSON Serialization Testing

Verify BSON serialization behavior:

```csharp
using MongoDB.Bson;
using AwesomeAssertions;

namespace YourProject.Integration.Tests.MongoDB;

public class MongoBsonTests
{
    [Fact]
    public void ObjectIdGeneration_ShouldGenerateValidObjectId()
    {
        // Arrange & Act
        var objectId = ObjectId.GenerateNewId();

        // Assert
        objectId.Should().NotBeNull();
        objectId.CreationTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        objectId.ToString().Should().HaveLength(24);
    }

    [Fact]
    public void BsonDocumentCreation_WhenPassingNullValue_ShouldHandleCorrectly()
    {
        // Arrange
        var doc = new BsonDocument
        {
            ["name"] = "John",
            ["email"] = BsonNull.Value,
            ["age"] = 25
        };

        // Act
        var json = doc.ToJson();

        // Assert
        json.Should().Contain("\"email\" : null");
        doc["email"].IsBsonNull.Should().BeTrue();
    }

    [Fact]
    public void BsonArrayOperation_WhenUsingComplexArray_ShouldHandleCorrectly()
    {
        // Arrange
        var skills = new BsonArray
        {
            new BsonDocument { ["name"] = "C#", ["level"] = 5 },
            new BsonDocument { ["name"] = "MongoDB", ["level"] = 3 }
        };

        var doc = new BsonDocument
        {
            ["userId"] = ObjectId.GenerateNewId(),
            ["skills"] = skills
        };

        // Act
        var skillsArray = doc["skills"].AsBsonArray;
        var firstSkill = skillsArray[0].AsBsonDocument;

        // Assert
        skillsArray.Should().HaveCount(2);
        firstSkill["name"].AsString.Should().Be("C#");
        firstSkill["level"].AsInt32.Should().Be(5);
    }
}
```

### MongoDB CRUD Testing

```csharp
using MongoDB.Driver;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;

namespace YourProject.Integration.Tests.MongoDB;

[Collection("MongoDb Collection")]
public class MongoUserServiceTests
{
    private readonly MongoUserService _mongoUserService;
    private readonly IMongoDatabase _database;
    private readonly FakeTimeProvider _fakeTimeProvider;

    public MongoUserServiceTests(MongoDbContainerFixture fixture)
    {
        _database = fixture.Database;
        _fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        
        // Create service instance
        _mongoUserService = new MongoUserService(
            _database, 
            Options.Create(new MongoDbSettings { UsersCollectionName = "users" }),
            NullLogger<MongoUserService>.Instance,
            _fakeTimeProvider);
    }

    [Fact]
    public async Task CreateUserAsync_InputValidUser_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var user = new UserDocument
        {
            Username = $"testuser_{Guid.NewGuid():N}",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Profile = new UserProfile
            {
                FirstName = "Test",
                LastName = "User",
                Bio = "Test user bio"
            }
        };

        // Act
        var result = await _mongoUserService.CreateUserAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().Be(_fakeTimeProvider.GetUtcNow().DateTime);
    }

    [Fact]
    public async Task GetUserByIdAsync_InputExistingId_ShouldReturnCorrectUser()
    {
        // Arrange
        var user = new UserDocument
        {
            Username = $"gettest_{Guid.NewGuid():N}",
            Email = $"gettest_{Guid.NewGuid():N}@example.com",
            Profile = new UserProfile { FirstName = "Get", LastName = "Test" }
        };
        var createdUser = await _mongoUserService.CreateUserAsync(user);

        // Act
        var result = await _mongoUserService.GetUserByIdAsync(createdUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task UpdateUserAsync_UsingOptimisticLocking_ShouldSuccessfullyUpdateVersion()
    {
        // Arrange
        var user = new UserDocument
        {
            Username = $"updatetest_{Guid.NewGuid():N}",
            Email = $"updatetest_{Guid.NewGuid():N}@example.com"
        };
        var createdUser = await _mongoUserService.CreateUserAsync(user);
        createdUser.Profile.Bio = "Updated bio";

        // Act
        var result = await _mongoUserService.UpdateUserAsync(createdUser);

        // Assert
        result.Should().NotBeNull();
        result!.Version.Should().Be(2);
        result.Profile.Bio.Should().Be("Updated bio");
    }

    [Fact]
    public async Task DeleteUserAsync_InputExistingId_ShouldDeleteUserSuccessfully()
    {
        // Arrange
        var user = new UserDocument
        {
            Username = $"deletetest_{Guid.NewGuid():N}",
            Email = $"deletetest_{Guid.NewGuid():N}@example.com"
        };
        var createdUser = await _mongoUserService.CreateUserAsync(user);

        // Act
        var result = await _mongoUserService.DeleteUserAsync(createdUser.Id);

        // Assert
        result.Should().BeTrue();
        
        var deletedUser = await _mongoUserService.GetUserByIdAsync(createdUser.Id);
        deletedUser.Should().BeNull();
    }
}
```

### MongoDB Index Testing

```csharp
using MongoDB.Driver;
using AwesomeAssertions;
using System.Diagnostics;

namespace YourProject.Integration.Tests.MongoDB;

[Collection("MongoDb Collection")]
public class MongoIndexTests
{
    private readonly IMongoCollection<UserDocument> _users;
    private readonly ITestOutputHelper _output;

    public MongoIndexTests(MongoDbContainerFixture fixture, ITestOutputHelper output)
    {
        _users = fixture.Database.GetCollection<UserDocument>("index_test_users");
        _output = output;
    }

    [Fact]
    public async Task CreateUniqueIndex_EmailUniqueIndex_ShouldPreventDuplicateInsertion()
    {
        // Arrange - Ensure collection is empty
        await _users.DeleteManyAsync(FilterDefinition<UserDocument>.Empty);

        // Create unique index
        var indexKeysDefinition = Builders<UserDocument>.IndexKeys.Ascending(u => u.Email);
        var indexOptions = new CreateIndexOptions { Unique = true };
        await _users.Indexes.CreateOneAsync(
            new CreateIndexModel<UserDocument>(indexKeysDefinition, indexOptions));

        var uniqueEmail = $"unique_{Guid.NewGuid():N}@example.com";
        var user1 = new UserDocument { Username = "user1", Email = uniqueEmail };
        var user2 = new UserDocument { Username = "user2", Email = uniqueEmail };

        // Act & Assert
        await _users.InsertOneAsync(user1); // First insertion succeeds

        var exception = await Assert.ThrowsAsync<MongoWriteException>(
            () => _users.InsertOneAsync(user2));
        exception.WriteError.Category.Should().Be(ServerErrorCategory.DuplicateKey);

        _output.WriteLine("Unique index test passed - duplicate email correctly blocked");
    }

    [Fact]
    public async Task CompoundIndex_CompoundIndexQueryPerformance_ShouldImproveQuerySpeed()
    {
        // Arrange - Ensure collection is empty
        await _users.DeleteManyAsync(FilterDefinition<UserDocument>.Empty);

        // Insert test data
        var testUsers = Enumerable.Range(0, 1000)
            .Select(i => new UserDocument
            {
                Username = $"user_{i:D4}",
                Email = $"user{i:D4}_{Guid.NewGuid():N}@example.com",
                IsActive = i % 2 == 0,
                CreatedAt = DateTime.UtcNow.AddDays(-i % 365)
            })
            .ToList();

        await _users.InsertManyAsync(testUsers);

        // Create compound index
        var compoundIndex = Builders<UserDocument>.IndexKeys
            .Ascending(u => u.IsActive)
            .Descending(u => u.CreatedAt);
        await _users.Indexes.CreateOneAsync(new CreateIndexModel<UserDocument>(compoundIndex));

        // Test query performance
        var filter = Builders<UserDocument>.Filter.And(
            Builders<UserDocument>.Filter.Eq(u => u.IsActive, true),
            Builders<UserDocument>.Filter.Gte(u => u.CreatedAt, DateTime.UtcNow.AddDays(-100))
        );

        var stopwatch = Stopwatch.StartNew();
        var results = await _users.Find(filter).ToListAsync();
        stopwatch.Stop();

        _output.WriteLine($"Query time: {stopwatch.ElapsedMilliseconds}ms, Result count: {results.Count}");
        
        // Assert
        results.Should().NotBeEmpty();
    }
}
```
