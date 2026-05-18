using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests;

public class QdrantServiceCollectionExtensionsTests
{
    private static readonly Type TargetType = typeof(QdrantServiceCollectionExtensions);
    private static readonly MethodInfo GetCollectionOptionsMethod = TargetType.GetMethod("GetCollectionOptions", 
        BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly MethodInfo GetStoreOptionsMethod = TargetType.GetMethod("GetStoreOptions", 
        BindingFlags.Static | BindingFlags.NonPublic)!;

    [Fact]
    public void GetCollectionOptions_NoOptionsProvider_ReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        dynamic result = GetCollectionOptionsMethod.Invoke(null, new object?[] { serviceProvider, null });

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_OptionsProviderReturnsNull_ReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => null;

        // Act
        dynamic result = GetCollectionOptionsMethod.Invoke(null, new object?[] { serviceProvider, optionsProvider });

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_OptionsWithEmbeddingGenerator_ReturnsSameOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        var options = new QdrantCollectionOptions { EmbeddingGenerator = mockEmbeddingGenerator.Object };
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        dynamic result = GetCollectionOptionsMethod.Invoke(null, new object?[] { serviceProvider, optionsProvider });

        // Assert
        Assert.Same(options, result);
        Assert.NotNull(((QdrantCollectionOptions)result).EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_NoEmbeddingInDI_ReturnsOriginalOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var options = new QdrantCollectionOptions();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        dynamic result = GetCollectionOptionsMethod.Invoke(null, new object?[] { serviceProvider, optionsProvider });

        // Assert
        Assert.Same(options, result);
        Assert.Null(((QdrantCollectionOptions)result).EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_EmbeddingInDI_ReturnsNewOptionsWithEmbedding()
    {
        // Arrange
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        var services = new ServiceCollection();
        services.AddSingleton(mockEmbeddingGenerator.Object);
        var serviceProvider = services.BuildServiceProvider();

        var options = new QdrantCollectionOptions();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        dynamic result = GetCollectionOptionsMethod.Invoke(null, new object?[] { serviceProvider, optionsProvider });

        // Assert
        Assert.NotSame(options, result);
        Assert.NotNull(((QdrantCollectionOptions)result).EmbeddingGenerator);
        Assert.Same(mockEmbeddingGenerator.Object, ((QdrantCollectionOptions)result).EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_NoOptionsProvider_ReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        dynamic result = GetStoreOptionsMethod.Invoke(null, new object?[] { serviceProvider, null });

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_OptionsWithEmbeddingGenerator_ReturnsSameOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        var options = new QdrantVectorStoreOptions { EmbeddingGenerator = mockEmbeddingGenerator.Object };
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        dynamic result = GetStoreOptionsMethod.Invoke(null, new object?[] { serviceProvider, optionsProvider });

        // Assert
        Assert.Same(options, result);
        Assert.NotNull(((QdrantVectorStoreOptions)result).EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_NoEmbeddingInDI_ReturnsOriginalOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var options = new QdrantVectorStoreOptions();
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        dynamic result = GetStoreOptionsMethod.Invoke(null, new object?[] { serviceProvider, optionsProvider });

        // Assert
        Assert.Same(options, result);
        Assert.Null(((QdrantVectorStoreOptions)result).EmbeddingGenerator);
    }

    [Fact]
    public void GetStoreOptions_EmbeddingInDI_ReturnsNewOptionsWithEmbedding()
    {
        // Arrange
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        var services = new ServiceCollection();
        services.AddSingleton(mockEmbeddingGenerator.Object);
        var serviceProvider = services.BuildServiceProvider();

        var options = new QdrantVectorStoreOptions();
        Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        dynamic result = GetStoreOptionsMethod.Invoke(null, new object?[] { serviceProvider, optionsProvider });

        // Assert
        Assert.NotSame(options, result);
        Assert.NotNull(((QdrantVectorStoreOptions)result).EmbeddingGenerator);
        Assert.Same(mockEmbeddingGenerator.Object, ((QdrantVectorStoreOptions)result).EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_VerifiesGetServiceCallOnIServiceProvider()
    {
        // Arrange - Create service provider with IEmbeddingGenerator
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        var services = new ServiceCollection();
        services.AddSingleton(mockEmbeddingGenerator.Object);
        var serviceProvider = services.BuildServiceProvider();

        var options = new QdrantCollectionOptions();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        dynamic result = GetCollectionOptionsMethod.Invoke(null, new object?[] { serviceProvider, optionsProvider });

        // Assert - Verifies the GetService call was made and embedding was retrieved
        Assert.NotSame(options, result);
        Assert.Same(mockEmbeddingGenerator.Object, ((QdrantCollectionOptions)result).EmbeddingGenerator);
    }
}
