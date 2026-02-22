namespace MyApp.Tests.Integration.Infrastructure;

/// <summary>
/// Integration test collection definition
/// Uses Collection Fixture to share AspireAppFixture across all test classes
/// Avoids restarting containers for each test class, improving test performance
/// </summary>
[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<AspireAppFixture>
{
    /// <summary>
    /// Test collection name
    /// </summary>
    public const string Name = "Integration Tests";

    // This class doesn't need to implement any code
    // It's just used to define the Collection Fixture
    // All test classes marked with [Collection("Integration Tests")]
    // will share the same AspireAppFixture instance
}

// Usage:
// [Collection(IntegrationTestCollection.Name)]
// public class MyControllerTests : IntegrationTestBase
// {
//     public MyControllerTests(AspireAppFixture fixture) : base(fixture) { }
// }
