using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests;

public class PostgresServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_ServiceProviderWithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<Microsoft.SemanticKernel.Connectors.PgVector.IEmbeddingGenerator>(Mock.Of<Microsoft.SemanticKernel.Connectors.PgVector.IEmbeddingGenerator>())
            .BuildServiceProvider();

        var optionsProvider = (IServiceProvider sp) => new PostgresVectorStoreOptions();

        // Act
        var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_ServiceProviderWithoutEmbeddingGenerator_ReturnsOptionsWithoutEmbeddingGenerator()
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
    public void GetStoreOptions_OptionsProviderReturnsOptionsWithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .BuildServiceProvider();

        var optionsProvider = (IServiceProvider sp) => new PostgresVectorStoreOptions { EmbeddingGenerator = Mock.Of<Microsoft.SemanticKernel.Connectors.PgVector.IEmbeddingGenerator>() };

        // Act
        var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_OptionsProviderReturnsOptionsWithoutEmbeddingGenerator_ReturnsOptionsWithoutEmbeddingGenerator()
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
}
