using StackExchange.Redis;
using AwesomeAssertions;
using System.Text.Json;
using Xunit;

namespace YourProject.Integration.Tests.Redis;

/// <summary>
/// Complete tests for Redis five data structures
/// String, Hash, List, Set, Sorted Set in-depth application
/// </summary>
[Collection("Redis Collection")]
public class RedisDataStructureTests
{
    private readonly RedisCacheService _redisCacheService;
    private readonly RedisContainerFixture _fixture;

    public RedisDataStructureTests(RedisContainerFixture fixture)
    {
        _fixture = fixture;
        _redisCacheService = new RedisCacheService(
            fixture.Connection,
            Options.Create(new RedisSettings()),
            NullLogger<RedisCacheService>.Instance,
            TimeProvider.System);
    }

    #region String Tests

    [Fact]
    public async Task String_SetAndGet_InputStringValue_ShouldSuccessfullySetAndGetCache()
    {
        // Arrange
        var key = $"test_string_{Guid.NewGuid():N}";
        var value = "test_string_value";

        // Act
        var setResult = await _redisCacheService.SetStringAsync(key, value);
        var getResult = await _redisCacheService.GetStringAsync<string>(key);

        // Assert
        setResult.Should().BeTrue();
        getResult.Should().Be(value);
    }

    [Fact]
    public async Task String_SetObject_InputComplexObject_ShouldCorrectlySerializeAndDeserialize()
    {
        // Arrange
        var key = $"object_test_{Guid.NewGuid():N}";
        var user = new UserDocument
        {
            Username = "objecttest",
            Email = "object@test.com",
            Profile = new UserProfile
            {
                FirstName = "Object",
                LastName = "Test",
                Bio = "A test user for object serialization"
            }
        };

        // Act
        var setResult = await _redisCacheService.SetStringAsync(key, user, TimeSpan.FromMinutes(30));
        var getResult = await _redisCacheService.GetStringAsync<UserDocument>(key);

        // Assert
        setResult.Should().BeTrue();
        getResult.Should().NotBeNull();
        getResult!.Username.Should().Be("objecttest");
        getResult.Email.Should().Be("object@test.com");
        getResult.Profile.FirstName.Should().Be("Object");
    }

    [Fact]
    public async Task String_SetMultiple_InputMultipleKeyValuePairs_ShouldSuccessfullyBatchSet()
    {
        // Arrange
        var prefix = Guid.NewGuid().ToString("N")[..8];
        var keyValues = new Dictionary<string, string>
        {
            { $"multi1_{prefix}", "value1" },
            { $"multi2_{prefix}", "value2" },
            { $"multi3_{prefix}", "value3" }
        };

        // Act
        var result = await _redisCacheService.SetMultipleStringAsync(keyValues);

        // Assert
        result.Should().BeTrue();
        foreach (var kvp in keyValues)
        {
            var value = await _redisCacheService.GetStringAsync<string>(kvp.Key);
            value.Should().Be(kvp.Value);
        }
    }

    [Fact]
    public async Task String_GetNonExistent_GettingNonExistingKey_ShouldReturnDefault()
    {
        // Arrange
        var key = $"nonexistent_{Guid.NewGuid():N}";

        // Act
        var result = await _redisCacheService.GetStringAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Hash Tests

    [Fact]
    public async Task Hash_SetAndGet_InputFieldValue_ShouldCorrectlySetAndGet()
    {
        // Arrange
        var key = $"hash_test_{Guid.NewGuid():N}";
        var field = "test_field";
        var value = "test_value";

        // Act
        var setResult = await _redisCacheService.SetHashAsync(key, field, value, TimeSpan.FromMinutes(30));
        var getResult = await _redisCacheService.GetHashAsync<string>(key, field);

        // Assert
        setResult.Should().BeTrue();
        getResult.Should().Be(value);
    }

    [Fact]
    public async Task Hash_SetAll_InputObject_ShouldSetCompleteHashStructure()
    {
        // Arrange
        var key = $"hash_all_{Guid.NewGuid():N}";
        var session = new UserSession
        {
            UserId = "user123",
            SessionId = "session456",
            IpAddress = "192.168.1.1",
            UserAgent = "Test Browser",
            IsActive = true
        };

        // Act
        var setResult = await _redisCacheService.SetHashAllAsync(key, session, TimeSpan.FromHours(1));
        var getResult = await _redisCacheService.GetHashAllAsync<UserSession>(key);

        // Assert
        setResult.Should().BeTrue();
        getResult.Should().NotBeNull();
        getResult!.UserId.Should().Be("user123");
        getResult.SessionId.Should().Be("session456");
        getResult.IpAddress.Should().Be("192.168.1.1");
        getResult.IsActive.Should().BeTrue();
    }

    #endregion

    #region List Tests

    [Fact]
    public async Task List_LeftPush_InputMultipleValues_ShouldStoreInLIFOOrder()
    {
        // Arrange
        var key = $"list_test_{Guid.NewGuid():N}";
        var view1 = new RecentView { ItemId = "item1", ItemType = "product", Title = "Product 1" };
        var view2 = new RecentView { ItemId = "item2", ItemType = "product", Title = "Product 2" };
        var view3 = new RecentView { ItemId = "item3", ItemType = "product", Title = "Product 3" };

        // Act
        await _redisCacheService.ListLeftPushAsync(key, view1);
        await _redisCacheService.ListLeftPushAsync(key, view2);
        await _redisCacheService.ListLeftPushAsync(key, view3);

        var views = await _redisCacheService.ListRangeAsync<RecentView>(key);

        // Assert - Last added is first (LIFO)
        views.Should().HaveCount(3);
        views[0].ItemId.Should().Be("item3");
        views[1].ItemId.Should().Be("item2");
        views[2].ItemId.Should().Be("item1");
    }

    [Fact]
    public async Task List_Trim_KeepSpecifiedRange_ShouldCorrectlyTruncateList()
    {
        // Arrange
        var key = $"list_trim_{Guid.NewGuid():N}";
        for (int i = 1; i <= 10; i++)
        {
            await _redisCacheService.ListLeftPushAsync(key, new RecentView
            {
                ItemId = $"item{i}",
                Title = $"Product {i}"
            });
        }

        // Act - Keep only the latest 5 items
        await _redisCacheService.ListTrimAsync(key, 0, 4);
        var views = await _redisCacheService.ListRangeAsync<RecentView>(key);

        // Assert
        views.Should().HaveCount(5);
        views[0].ItemId.Should().Be("item10");  // Latest
        views[4].ItemId.Should().Be("item6");   // 5th item
    }

    #endregion

    #region Set Tests

    [Fact]
    public async Task Set_Add_UniqueValues_ShouldCorrectlyAdd()
    {
        // Arrange
        var key = $"set_test_{Guid.NewGuid():N}";
        var tag1 = "programming";
        var tag2 = "testing";
        var tag3 = "dotnet";

        // Act
        var result1 = await _redisCacheService.SetAddAsync(key, tag1);
        var result2 = await _redisCacheService.SetAddAsync(key, tag2);
        var result3 = await _redisCacheService.SetAddAsync(key, tag3);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();

        var tags = await _redisCacheService.SetMembersAsync<string>(key);
        tags.Should().HaveCount(3);
        tags.Should().Contain("programming");
        tags.Should().Contain("testing");
        tags.Should().Contain("dotnet");
    }

    [Fact]
    public async Task Set_AddDuplicate_InputDuplicateValue_ShouldReturnFalse()
    {
        // Arrange
        var key = $"set_dup_{Guid.NewGuid():N}";
        var tag = "programming";

        // Act
        var result1 = await _redisCacheService.SetAddAsync(key, tag);
        var result2 = await _redisCacheService.SetAddAsync(key, tag);  // Duplicate

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();  // Duplicate returns false

        var tags = await _redisCacheService.SetMembersAsync<string>(key);
        tags.Should().HaveCount(1);  // Only one member
    }

    [Fact]
    public async Task Set_IsMember_CheckMemberExists_ShouldReturnCorrectResult()
    {
        // Arrange
        var key = $"set_member_{Guid.NewGuid():N}";
        await _redisCacheService.SetAddAsync(key, "exists");

        // Act & Assert
        var exists = await _redisCacheService.SetContainsAsync(key, "exists");
        var notExists = await _redisCacheService.SetContainsAsync(key, "not_exists");

        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }

    #endregion

    #region Sorted Set Tests

    [Fact]
    public async Task SortedSet_Add_InputScoreAndMember_ShouldSortByScore()
    {
        // Arrange
        var key = $"zset_test_{Guid.NewGuid():N}";
        var entry1 = new LeaderboardEntry { UserId = "user1", Username = "Player1", Score = 100 };
        var entry2 = new LeaderboardEntry { UserId = "user2", Username = "Player2", Score = 200 };
        var entry3 = new LeaderboardEntry { UserId = "user3", Username = "Player3", Score = 150 };

        // Act
        await _redisCacheService.SortedSetAddAsync(key, entry1, entry1.Score);
        await _redisCacheService.SortedSetAddAsync(key, entry2, entry2.Score);
        await _redisCacheService.SortedSetAddAsync(key, entry3, entry3.Score);

        // Get in descending order (highest score first)
        var rankings = await _redisCacheService.SortedSetRangeWithScoresAsync<LeaderboardEntry>(
            key, 0, -1, Order.Descending);

        // Assert
        rankings.Should().HaveCount(3);
        rankings[0].Member.Username.Should().Be("Player2");  // 200 points
        rankings[0].Score.Should().Be(200);
        rankings[1].Member.Username.Should().Be("Player3");  // 150 points
        rankings[2].Member.Username.Should().Be("Player1");  // 100 points
    }

    [Fact]
    public async Task SortedSet_IncrementScore_UpdateScore_ShouldCorrectlyAccumulate()
    {
        // Arrange
        var key = $"zset_incr_{Guid.NewGuid():N}";
        var entry = new LeaderboardEntry { UserId = "user1", Username = "Player1", Score = 100 };
        await _redisCacheService.SortedSetAddAsync(key, entry, entry.Score);

        // Act - Add 50 points
        var newScore = await _redisCacheService.SortedSetIncrementAsync(key, entry, 50);

        // Assert
        newScore.Should().Be(150);
    }

    [Fact]
    public async Task SortedSet_Rank_GetRank_ShouldReturnCorrectRank()
    {
        // Arrange
        var key = $"zset_rank_{Guid.NewGuid():N}";
        await _redisCacheService.SortedSetAddAsync(key, "player1", 100);
        await _redisCacheService.SortedSetAddAsync(key, "player2", 200);
        await _redisCacheService.SortedSetAddAsync(key, "player3", 150);

        // Act - Descending rank (higher score = better rank)
        var rank = await _redisCacheService.SortedSetRankAsync(key, "player2", Order.Descending);

        // Assert - player2 (200 points) should be rank 1 (index 0)
        rank.Should().Be(0);
    }

    #endregion

    #region TTL and Expiration Tests

    [Fact]
    public async Task Expire_SetExpirationTime_ShouldCorrectlySetTTL()
    {
        // Arrange
        var key = $"expire_test_{Guid.NewGuid():N}";
        await _redisCacheService.SetStringAsync(key, "expire_value");

        // Act
        var result = await _redisCacheService.ExpireAsync(key, TimeSpan.FromMinutes(5));

        // Assert
        result.Should().BeTrue();
        var ttl = await _redisCacheService.GetTtlAsync(key);
        ttl.Should().NotBeNull();
        ttl!.Value.TotalMinutes.Should().BeGreaterThan(4);
    }

    [Fact]
    public async Task KeyExists_CheckKeyExists_ShouldReturnCorrectResult()
    {
        // Arrange
        var existingKey = $"exists_{Guid.NewGuid():N}";
        var nonExistingKey = $"not_exists_{Guid.NewGuid():N}";
        await _redisCacheService.SetStringAsync(existingKey, "value");

        // Act & Assert
        var exists = await _redisCacheService.ExistsAsync(existingKey);
        var notExists = await _redisCacheService.ExistsAsync(nonExistingKey);

        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }

    #endregion

    #region Data Isolation Tests

    [Fact]
    public async Task DataIsolation_UsingUniqueKeyPrefix_TestsShouldNotAffectEachOther()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
        var key1 = $"isolation:{testId}:key1";
        var key2 = $"isolation:{testId}:key2";

        // Act
        await _redisCacheService.SetStringAsync(key1, "value1");
        await _redisCacheService.SetStringAsync(key2, "value2");

        // Assert
        var value1 = await _redisCacheService.GetStringAsync<string>(key1);
        var value2 = await _redisCacheService.GetStringAsync<string>(key2);

        value1.Should().Be("value1");
        value2.Should().Be("value2");

        // Cleanup
        await _redisCacheService.DeleteAsync(key1);
        await _redisCacheService.DeleteAsync(key2);
    }

    #endregion

    #region Key Pattern Search Tests

    [Fact]
    public async Task SearchKeys_InputPattern_ShouldReturnMatchingKeys()
    {
        // Arrange
        var prefix = Guid.NewGuid().ToString("N")[..8];
        await _redisCacheService.SetStringAsync($"search:{prefix}:test1", "value1");
        await _redisCacheService.SetStringAsync($"search:{prefix}:test2", "value2");
        await _redisCacheService.SetStringAsync($"other:{prefix}:test", "value3");

        // Act
        var result = await _redisCacheService.SearchKeysAsync($"search:{prefix}:*");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain($"search:{prefix}:test1");
        result.Should().Contain($"search:{prefix}:test2");

        // Cleanup
        await _redisCacheService.DeleteAsync($"search:{prefix}:test1");
        await _redisCacheService.DeleteAsync($"search:{prefix}:test2");
        await _redisCacheService.DeleteAsync($"other:{prefix}:test");
    }

    #endregion
}

#region Test Models

/// <summary>
/// User Session - Hash structure example
/// </summary>
public class UserSession
{
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// Recent view history - List structure example
/// </summary>
public class RecentView
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Leaderboard entry - Sorted Set structure example
/// </summary>
public class LeaderboardEntry
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public double Score { get; set; }
}

#endregion
