using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.Tests;

public class QdrantServiceCollectionExtensionsTests
{
    private static T? InvokeStaticMethod<T>(string methodName, params object?[] args)
    {
        var type = typeof(QdrantServiceCollectionExtensions);
        var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!;
        return (T?)method.Invoke(null, args);
    }

    [Fact]
    public void GetCollectionOptions_WithNullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = InvokeStaticMethod<QdrantCollectionOptions?>("GetCollectionOptions", serviceProvider, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_WithOptionsProviderReturningNull_ReturnsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => null;

        // Act
        var result = InvokeStaticMethod<QdrantCollectionOptions?>("GetCollectionOptions", serviceProvider, optionsProvider);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_WithOptionsHavingEmbeddingGenerator_ReturnsSameOptions()
    {
        // Arrange
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        var options = new QdrantCollectionOptions { EmbeddingGenerator = mockEmbeddingGenerator.Object };
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = InvokeStaticMethod<QdrantCollectionOptions?>("GetCollectionOptions", serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
        Assert.NotNull(result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_NoEmbeddingGeneratorInDI_ReturnsOriginalOptions()
    {
        // Arrange
        var options = new QdrantCollectionOptions();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = InvokeStaticMethod<QdrantCollectionOptions?>("GetCollectionOptions", serviceProvider, optionsProvider);

        // Assert
        Assert.Same(options, result);
        Assert.Null(result!.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_EmbeddingGeneratorInDI_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(mockEmbeddingGenerator.Object);
        var serviceProvider = services.BuildServiceProvider();

        var originalOptions = new QdrantCollectionOptions();
        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => originalOptions;

        // Act
        var result = InvokeStaticMethod<QdrantCollectionOptions?>("GetCollectionOptions", serviceProvider, optionsProvider);

        // Assert
        Assert.NotSame(originalOptions, result);
        Assert.NotNull(result!.EmbeddingGenerator);
        Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
    }

    [Fact]
    public void GetCollectionOptions_EmbeddingGeneratorInDIWithNullOriginalOptions_ReturnsNewOptionsWithGenerator()
    {
        // Arrange
        var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(mockEmbeddingGenerator.Object);
        var serviceProvider = services.BuildServiceProvider();

        Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => null;

        // Act
        var result = InvokeStaticMethod<QdrantCollectionOptions?>("GetCollectionOptions", serviceProvider, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.EmbeddingGenerator);
        Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
    }
}
