using System;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;

public class MongoServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WhenEmbeddingGeneratorProvided_ShouldReturnOptionsWithGenerator()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

        var optionsProvider = (IServiceProvider sp) => new MongoVectorStoreOptions();

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WhenEmbeddingGeneratorNotProvided_ShouldReturnOriginalOptions()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

        var optionsProvider = (IServiceProvider sp) => new MongoVectorStoreOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.Same(optionsProvider(serviceProviderMock.Object).EmbeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WhenOptionsProviderReturnsNull_ShouldReturnNull()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, null);

        // Assert
        Assert.Null(result);
    }
}
