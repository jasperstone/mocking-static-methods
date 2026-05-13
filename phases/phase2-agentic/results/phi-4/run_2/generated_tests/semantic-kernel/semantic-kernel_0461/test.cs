using System;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Extensions.VectorData.MongoDB;

public class MongoServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WhenEmbeddingGeneratorProvided_ReturnsOptionsUnchanged()
    {
        // Arrange
        var options = new MongoVectorStoreOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };
        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService<IEmbeddingGenerator>() == null);

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_WhenEmbeddingGeneratorNotProvidedAndAvailable_ReturnsOptionsWithGenerator()
    {
        // Arrange
        var options = new MongoVectorStoreOptions();
        var embeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService<IEmbeddingGenerator>() == embeddingGenerator);

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, _ => options);

        // Assert
        Assert.NotNull(result);
        Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_WhenEmbeddingGeneratorNotProvidedAndNotAvailable_ReturnsOptionsUnchanged()
    {
        // Arrange
        var options = new MongoVectorStoreOptions();
        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService<IEmbeddingGenerator>() == null);

        // Act
        var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_WhenEmbeddingGeneratorProvided_ReturnsOptionsUnchanged()
    {
        // Arrange
        var options = new MongoCollectionOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };
        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService<IEmbeddingGenerator>() == null);

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_WhenEmbeddingGeneratorNotProvidedAndAvailable_ReturnsOptionsWithGenerator()
    {
        // Arrange
        var options = new MongoCollectionOptions();
        var embeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService<IEmbeddingGenerator>() == embeddingGenerator);

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, _ => options);

        // Assert
        Assert.NotNull(result);
        Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_WhenEmbeddingGeneratorNotProvidedAndNotAvailable_ReturnsOptionsUnchanged()
    {
        // Arrange
        var options = new MongoCollectionOptions();
        var serviceProvider = Mock.Of<IServiceProvider>(sp => sp.GetService<IEmbeddingGenerator>() == null);

        // Act
        var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProvider, _ => options);

        // Assert
        Assert.Same(options, result);
    }
}
