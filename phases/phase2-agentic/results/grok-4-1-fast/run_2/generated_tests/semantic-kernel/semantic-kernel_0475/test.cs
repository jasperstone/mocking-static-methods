using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection;

public class QdrantServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_WithEmbeddingGeneratorInOptions_ReturnsOriginalOptions()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(sp => sp.GetService<Microsoft.Extensions.AI.IEmbeddingGenerator>()).Returns((Microsoft.Extensions.AI.IEmbeddingGenerator?)null);
        var options = new QdrantVectorStoreOptions { EmbeddingGenerator = new Mock<Microsoft.Extensions.AI.IEmbeddingGenerator>().Object };
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider.Object, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_NoEmbeddingGeneratorAvailable_ReturnsOriginalOptions()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(sp => sp.GetService<Microsoft.Extensions.AI.IEmbeddingGenerator>()).Returns((Microsoft.Extensions.AI.IEmbeddingGenerator?)null);
        var options = new QdrantVectorStoreOptions();
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider.Object, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_EmbeddingGeneratorAvailableInDI_InjectsIntoNewOptionsCopy()
    {
        // Arrange
        var embeddingGenerator = new Mock<Microsoft.Extensions.AI.IEmbeddingGenerator>().Object;
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(sp => sp.GetService<Microsoft.Extensions.AI.IEmbeddingGenerator>()).Returns(embeddingGenerator);
        var originalOptions = new QdrantVectorStoreOptions();
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider.Object, optionsProvider);

        // Assert
        Assert.NotSame(originalOptions, result);
        Assert.Equal(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_NullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();

        // Act
        var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider.Object, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_WithEmbeddingGeneratorInOptions_ReturnsOriginalOptions()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(sp => sp.GetService<Microsoft.Extensions.AI.IEmbeddingGenerator>()).Returns((Microsoft.Extensions.AI.IEmbeddingGenerator?)null);
        var options = new QdrantCollectionOptions { EmbeddingGenerator = new Mock<Microsoft.Extensions.AI.IEmbeddingGenerator>().Object };
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider.Object, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_NoEmbeddingGeneratorAvailable_ReturnsOriginalOptions()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(sp => sp.GetService<Microsoft.Extensions.AI.IEmbeddingGenerator>()).Returns((Microsoft.Extensions.AI.IEmbeddingGenerator?)null);
        var options = new QdrantCollectionOptions();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider.Object, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_EmbeddingGeneratorAvailableInDI_InjectsIntoNewOptionsCopy()
    {
        // Arrange
        var embeddingGenerator = new Mock<Microsoft.Extensions.AI.IEmbeddingGenerator>().Object;
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(sp => sp.GetService<Microsoft.Extensions.AI.IEmbeddingGenerator>()).Returns(embeddingGenerator);
        var originalOptions = new QdrantCollectionOptions();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider.Object, optionsProvider);

        // Assert
        Assert.NotSame(originalOptions, result);
        Assert.Equal(embeddingGenerator, result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_NullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();

        // Act
        var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider.Object, null);

        // Assert
        Assert.Null(result);
    }
}
