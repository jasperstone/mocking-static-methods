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
        var serviceProviderMock = new Mock<IServiceProvider>();
        var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

        var options = new PostgresVectorStoreOptions();

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, _ => options);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(options, result);
        Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_ServiceProviderDoesNotHaveEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

        var options = new PostgresVectorStoreOptions();

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, _ => options);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_OptionsAlreadyHasEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

        var options = new PostgresVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };

        // Act
        var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, _ => options);

        // Assert
        Assert.Same(options, result);
    }
}
