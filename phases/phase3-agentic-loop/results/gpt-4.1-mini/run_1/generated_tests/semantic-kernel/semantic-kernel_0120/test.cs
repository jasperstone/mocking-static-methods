using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.ImageToText;
using Microsoft.SemanticKernel.TextGeneration;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.HuggingFace;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceTextGeneration_WithModel_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactoryMock = new Mock<ILoggerFactory>();

        // Register ILoggerFactory so that GetService<ILoggerFactory>() returns the mock
        services.AddSingleton(loggerFactoryMock.Object);

        var httpClient = new HttpClient();

        // Act
        var result = services.AddHuggingFaceTextGeneration(
            model: "test-model",
            endpoint: new Uri("http://localhost"),
            apiKey: "api-key",
            serviceId: "service-id",
            httpClient: httpClient);

        // Build service provider to invoke the factory delegate
        var builtProvider = services.BuildServiceProvider();

        // Resolve the service to trigger the factory and thus the GetService call
        var service = builtProvider.GetService<ITextGenerationService>();

        // Assert
        Assert.Same(services, result);
        Assert.NotNull(service);
    }

    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_WithModel_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactoryMock = new Mock<ILoggerFactory>();

        // Register ILoggerFactory so that GetService<ILoggerFactory>() returns the mock
        services.AddSingleton(loggerFactoryMock.Object);

        var httpClient = new HttpClient();

        // Act
        var result = services.AddHuggingFaceTextEmbeddingGeneration(
            model: "test-model",
            endpoint: new Uri("http://localhost"),
            apiKey: "api-key",
            serviceId: "service-id",
            httpClient: httpClient);

        // Build service provider to invoke the factory delegate
        var builtProvider = services.BuildServiceProvider();

        // Resolve the service to trigger the factory and thus the GetService call
        var service = builtProvider.GetService<ITextEmbeddingGenerationService>();

        // Assert
        Assert.Same(services, result);
        Assert.NotNull(service);
    }

    [Fact]
    public void AddHuggingFaceImageToText_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactoryMock = new Mock<ILoggerFactory>();

        // Register ILoggerFactory so that GetService<ILoggerFactory>() returns the mock
        services.AddSingleton(loggerFactoryMock.Object);

        var httpClient = new HttpClient();

        // Act
        var result = services.AddHuggingFaceImageToText(
            model: "test-model",
            endpoint: new Uri("http://localhost"),
            apiKey: "api-key",
            serviceId: "service-id",
            httpClient: httpClient);

        // Build service provider to invoke the factory delegate
        var builtProvider = services.BuildServiceProvider();

        // Resolve the service to trigger the factory and thus the GetService call
        var service = builtProvider.GetService<IImageToTextService>();

        // Assert
        Assert.Same(services, result);
        Assert.NotNull(service);
    }
}
