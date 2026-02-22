using MongoDB.Bson;
using MongoDB.Driver;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;
using Xunit.Abstractions;

namespace YourProject.Integration.Tests.MongoDB;

/// <summary>
/// MongoDB CRUD Tests - Demonstrates complete document operation testing
/// Uses Collection Fixture to share container for test performance
/// </summary>
[Collection("MongoDb Collection")]
public class MongoUserServiceTests
{
    private readonly MongoUserService _mongoUserService;
    private readonly IMongoDatabase _database;
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly MongoDbContainerFixture _fixture;

    public MongoUserServiceTests(MongoDbContainerFixture fixture)
    {
        _fixture = fixture;
        _database = fixture.Database;
        _fakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        // Create service instance (in production, should use DI)
        _mongoUserService = new MongoUserService(
            _database,
            Options.Create(new MongoDbSettings { UsersCollectionName = "users" }),
            NullLogger<MongoUserService>.Instance,
            _fakeTimeProvider);
    }

    #region Create Tests

    [Fact]
    public async Task CreateUserAsync_ValidInput_ShouldSuccessfullyCreateUser()
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
        result.Version.Should().Be(1);
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid():N}@example.com";
        var user1 = new UserDocument
        {
            Username = $"user1_{Guid.NewGuid():N}",
            Email = email
        };
        var user2 = new UserDocument
        {
            Username = $"user2_{Guid.NewGuid():N}",
            Email = email  // Same email
        };

        await _mongoUserService.CreateUserAsync(user1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _mongoUserService.CreateUserAsync(user2));
    }

    #endregion

    #region Read Tests

    [Fact]
    public async Task GetUserByIdAsync_ExistingId_ShouldReturnCorrectUser()
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
        result.Profile.FirstName.Should().Be("Get");
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistingId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = ObjectId.GenerateNewId().ToString();

        // Act
        var result = await _mongoUserService.GetUserByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByEmailAsync_ExistingEmail_ShouldReturnCorrectUser()
    {
        // Arrange
        var email = $"emailtest_{Guid.NewGuid():N}@example.com";
        var user = new UserDocument
        {
            Username = $"emailtest_{Guid.NewGuid():N}",
            Email = email,
            Profile = new UserProfile { FirstName = "Email", LastName = "Test" }
        };
        await _mongoUserService.CreateUserAsync(user);

        // Act
        var result = await _mongoUserService.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    #endregion

    #region Update Tests

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
    public async Task UpdateUserAsync_VersionMismatch_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = new UserDocument
        {
            Username = $"versiontest_{Guid.NewGuid():N}",
            Email = $"versiontest_{Guid.NewGuid():N}@example.com"
        };
        var createdUser = await _mongoUserService.CreateUserAsync(user);

        // Simulate another user updating this document first
        createdUser.Version = 0;  // Wrong version number

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _mongoUserService.UpdateUserAsync(createdUser));
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteUserAsync_ExistingId_ShouldSuccessfullyDeleteUser()
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

    [Fact]
    public async Task DeleteUserAsync_NonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = ObjectId.GenerateNewId().ToString();

        // Act
        var result = await _mongoUserService.DeleteUserAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}

/// <summary>
/// MongoDB BSON serialization tests - Validates BSON serialization behavior
/// </summary>
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
    public void BsonArrayOperations_WhenUsingComplexArrays_ShouldHandleCorrectly()
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

/// <summary>
/// MongoDB index tests - Validates index creation and performance
/// </summary>
[Collection("MongoDb Collection")]
public class MongoIndexTests
{
    private readonly MongoDbContainerFixture _fixture;
    private readonly IMongoCollection<UserDocument> _users;
    private readonly ITestOutputHelper _output;

    public MongoIndexTests(MongoDbContainerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _users = fixture.Database.GetCollection<UserDocument>("index_test_users");
        _output = output;
    }

    [Fact]
    public async Task CreateUniqueIndex_EmailUniqueIndex_ShouldPreventDuplicateInsertion()
    {
        // Arrange - ensure collection is empty
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
        await _users.InsertOneAsync(user1);  // First insert succeeds

        var exception = await Assert.ThrowsAsync<MongoWriteException>(
            () => _users.InsertOneAsync(user2));
        exception.WriteError.Category.Should().Be(ServerErrorCategory.DuplicateKey);

        _output.WriteLine("Unique index test passed - duplicate email correctly blocked");
    }
}
