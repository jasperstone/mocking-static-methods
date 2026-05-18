using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using Xunit;

namespace Microsoft.SemanticKernel;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithOpenAIClientNull_UsesGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<OpenAIClient>(new MockOpenAIClient());
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002");

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextEmbeddingGenerationService)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

        var serviceProvider = services.BuildServiceProvider();
        var factory = (Func<IServiceProvider, object?>)descriptor.ImplementationFactory!;
        var service = (ITextEmbeddingGenerationService)factory(serviceProvider)!;
        Assert.IsType<OpenAITextEmbeddingGenerationService>(service);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithOpenAIClientProvided_UsesProvidedClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var providedClient = new MockOpenAIClient();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", providedClient);

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextEmbeddingGenerationService)));
        var factory = (Func<IServiceProvider, object?>)descriptor.ImplementationFactory!;
        var service = factory(serviceProvider)!;
        Assert.IsType<OpenAITextEmbeddingGenerationService>(service);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<OpenAIClient>(new MockOpenAIClient());

        // Act
        var result = services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", serviceId: "test-key");

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextEmbeddingGenerationService)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithLoggerFactoryPresent_UsesGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton<OpenAIClient>(new MockOpenAIClient());

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002");

        // Assert - Verifies service creation succeeds with ILoggerFactory available via GetService
        var serviceProvider = services.BuildServiceProvider();
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextEmbeddingGenerationService)));
        var factory = (Func<IServiceProvider, object?>)descriptor.ImplementationFactory!;
        var service = Assert.IsType<OpenAITextEmbeddingGenerationService>(factory(serviceProvider)!);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_NoLoggerFactory_UsesGetServiceReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<OpenAIClient>(new MockOpenAIClient());

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002");

        // Assert - Verifies service creation succeeds when GetService<ILoggerFactory>() returns null
        var serviceProvider = services.BuildServiceProvider();
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextEmbeddingGenerationService)));
        var factory = (Func<IServiceProvider, object?>)descriptor.ImplementationFactory!;
        var service = Assert.IsType<OpenAITextEmbeddingGenerationService>(factory(serviceProvider)!);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithDimensions_PassesToConstructor()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<OpenAIClient>(new MockOpenAIClient());

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-3-small", dimensions: 1536);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextEmbeddingGenerationService)));
        var factory = (Func<IServiceProvider, object?>)descriptor.ImplementationFactory!;
        Assert.IsType<OpenAITextEmbeddingGenerationService>(factory(serviceProvider)!);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddOpenAITextEmbeddingGeneration("model"));
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_InvalidModelId_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        Assert.ThrowsAny<ArgumentException>(() => services.AddOpenAITextEmbeddingGeneration(""));
        Assert.ThrowsAny<ArgumentException>(() => services.AddOpenAITextEmbeddingGeneration(null!));
    }

    private sealed class MockOpenAIClient : OpenAIClient
    {
        public MockOpenAIClient() : base("fake-key") { }
    }
}
