namespace Ai.Tutor.Api.IntegrationTests;

using Xunit;

/// <summary>
/// Defines test collections to control test execution order and prevent database conflicts.
/// All integration tests that use the database should be in the same collection to run sequentially.
/// </summary>
[CollectionDefinition("Database Integration Tests")]
public class DatabaseTestCollection : ICollectionFixture<TestWebAppFactory>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
