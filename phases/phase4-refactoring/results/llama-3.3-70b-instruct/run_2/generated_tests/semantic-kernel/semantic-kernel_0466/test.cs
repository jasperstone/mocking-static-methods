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
        var serviceProviderMock = new Mock<IServiceProvider>();
        var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

        // Act
        var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, null);

        // Assert
        Assert.NotNull(options);
        Assert.Same(embeddingGeneratorMock.Object, options.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_ServiceProviderWithoutEmbeddingGenerator_ReturnsOptionsWithoutEmbeddingGenerator()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

        // Act
        var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, null);

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_OptionsProvider_ReturnsOptionsFromProvider()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var optionsProviderMock = new Mock<Func<IServiceProvider, PostgresVectorStoreOptions?>>();
        var expectedOptions = new PostgresVectorStoreOptions();
        optionsProviderMock.Setup(op => op(serviceProviderMock.Object)).Returns(expectedOptions);

        // Act
        var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProviderMock.Object);

        // Assert
        Assert.Same(expectedOptions, options);
    }
}
