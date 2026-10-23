using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests;

public class PostgresServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPostgresVectorStore_ServiceProviderWithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPostgresVectorStore(new PostgresVectorStoreOptions());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var postgresVectorStore = serviceProvider.GetService<PostgresVectorStore>();
        Assert.NotNull(postgresVectorStore);
    }
}
