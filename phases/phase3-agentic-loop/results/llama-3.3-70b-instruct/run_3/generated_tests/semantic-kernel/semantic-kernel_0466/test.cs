using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests;

public class PostgresServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_ServiceProviderHasEmbeddingGenerator_ReturnsNewOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<Microsoft.Extensions.AI.IEmbeddingGenerator>(Mock.Of<Microsoft.Extensions.AI.IEmbeddingGenerator>())
            .BuildServiceProvider();

        var optionsProvider = (IServiceProvider sp) => new PostgresVectorStoreOptions();

        // Act
        var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_ServiceProviderDoesNotHaveEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .BuildServiceProvider();

        var optionsProvider = (IServiceProvider sp) => new PostgresVectorStoreOptions();

        // Act
        var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_OptionsProviderIsNull_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .BuildServiceProvider();

        // Act
        var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, null);

        // Assert
        Assert.Null(options);
    }
}
