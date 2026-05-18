using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class PostgresServiceCollectionExtensionsTests
{
    [Fact]
    public void GetStoreOptions_NullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        // Act
        var result = PrivateType.GetStoreOptions(sp, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetStoreOptions_OptionsHasEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var options = new PostgresVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };
        Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = _ => options;
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        // Act
        var result = PrivateType.GetStoreOptions(sp, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_NoEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var options = new PostgresVectorStoreOptions();
        Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = _ => options;
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        // Act
        var result = PrivateType.GetStoreOptions(sp, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetStoreOptions_EmbeddingGeneratorInContainer_CreatesNewOptionsWithGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var sp = services.BuildServiceProvider();

        var options = new PostgresVectorStoreOptions();
        Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = _ => options;

        // Act
        var result = PrivateType.GetStoreOptions(sp, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(options, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
        Assert.Equal("public", result.Schema);
    }

    [Fact]
    public void GetCollectionOptions_NullOptionsProvider_ReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        // Act
        var result = PrivateType.GetCollectionOptions(sp, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCollectionOptions_OptionsHasEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var options = new PostgresCollectionOptions { EmbeddingGenerator = embeddingGenerator };
        Func<IServiceProvider, PostgresCollectionOptions?> optionsProvider = _ => options;
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        // Act
        var result = PrivateType.GetCollectionOptions(sp, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_NoEmbeddingGenerator_ReturnsOriginalOptions()
    {
        // Arrange
        var options = new PostgresCollectionOptions();
        Func<IServiceProvider, PostgresCollectionOptions?> optionsProvider = _ => options;
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        // Act
        var result = PrivateType.GetCollectionOptions(sp, optionsProvider);

        // Assert
        Assert.Same(options, result);
    }

    [Fact]
    public void GetCollectionOptions_EmbeddingGeneratorInContainer_CreatesNewOptionsWithGenerator()
    {
        // Arrange
        var embeddingGenerator = new MockEmbeddingGenerator();
        var services = new ServiceCollection();
        services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
        var sp = services.BuildServiceProvider();

        var options = new PostgresCollectionOptions();
        Func<IServiceProvider, PostgresCollectionOptions?> optionsProvider = _ => options;

        // Act
        var result = PrivateType.GetCollectionOptions(sp, optionsProvider);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(options, result);
        Assert.Same(embeddingGenerator, result!.EmbeddingGenerator);
    }
}

internal static class PrivateType
{
    private static readonly Lazy<Type> s_postgresExtensionsType = new(InitializeType);

    private static Type InitializeType()
    {
        var assembly = typeof(PostgresVectorStoreOptions).Assembly;
        return assembly.GetType("Microsoft.Extensions.DependencyInjection.PostgresServiceCollectionExtensions")!;
    }

    public static PostgresVectorStoreOptions? GetStoreOptions(IServiceProvider sp, Func<IServiceProvider, PostgresVectorStoreOptions?>? optionsProvider)
    {
        var method = s_postgresExtensionsType.Value.GetMethod("GetStoreOptions", 
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        return (PostgresVectorStoreOptions?)method.Invoke(null, [sp, optionsProvider]);
    }

    public static PostgresCollectionOptions? GetCollectionOptions(IServiceProvider sp, Func<IServiceProvider, PostgresCollectionOptions?>? optionsProvider)
    {
        var method = s_postgresExtensionsType.Value.GetMethod("GetCollectionOptions", 
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        return (PostgresCollectionOptions?)method.Invoke(null, [sp, optionsProvider]);
    }
}

internal sealed class MockEmbeddingGenerator : IEmbeddingGenerator, IDisposable
{
    public IReadOnlyList<Embedding> GenerateEmbeddings(IReadOnlyList<string> texts) => Array.Empty<Embedding>();
    public IAsyncEnumerable<Embedding> GenerateEmbeddingsAsync(IAsyncEnumerable<string> texts, CancellationToken cancellationToken = default)
        => System.Linq.AsyncEnumerable.Empty<Embedding>();
    public object? GetService(Type serviceType, object? serviceKey) => null;
    public void Dispose() { }
}
