using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using Moq;
using OpenAI;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.OpenAI.Extensions;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_UsesServiceProviderGetServiceForLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockOpenAIClient = new Mock<OpenAIClient>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);
        serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(OpenAIClient))).Returns(mockOpenAIClient.Object);

        services.AddSingleton(serviceProviderMock.Object);

        // Act
        var result = services.AddOpenAITextEmbeddingGeneration(
            modelId: "test-model",
            openAIClient: mockOpenAIClient.Object,
            serviceId: "test-service",
            dimensions: 123);

        // The factory delegate is stored internally, so we invoke it manually to test the call to GetService
        var serviceProvider = serviceProviderMock.Object;
        var factory = result.BuildServiceProvider().GetService<ITextEmbeddingGenerationService>();

        // Instead, we test the factory delegate directly by invoking the AddKeyedSingleton delegate
        // We need to get the delegate from the service collection
        var descriptor = Assert.Single(result, d => d.ServiceType == typeof(ITextEmbeddingGenerationService));
        var factoryDelegate = descriptor.ImplementationFactory;
        Assert.NotNull(factoryDelegate);

        var serviceInstance = factoryDelegate(serviceProvider, null);
        Assert.NotNull(serviceInstance);
        Assert.IsType<OpenAITextEmbeddingGenerationService>(serviceInstance);

        // Verify that GetService was called for ILoggerFactory
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithoutOpenAIClient_UsesServiceProviderGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockOpenAIClient = new Mock<OpenAIClient>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(OpenAIClient))).Returns(mockOpenAIClient.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

        services.AddSingleton(serviceProviderMock.Object);

        // Act
        var result = services.AddOpenAITextEmbeddingGeneration(
            modelId: "test-model",
            openAIClient: null,
            serviceId: "test-service",
            dimensions: 456);

        // Get the factory delegate from the service collection
        var descriptor = Assert.Single(result, d => d.ServiceType == typeof(ITextEmbeddingGenerationService));
        var factoryDelegate = descriptor.ImplementationFactory;
        Assert.NotNull(factoryDelegate);

        var serviceInstance = factoryDelegate(serviceProviderMock.Object, null);
        Assert.NotNull(serviceInstance);
        Assert.IsType<OpenAITextEmbeddingGenerationService>(serviceInstance);

        // Verify that GetRequiredService was called for OpenAIClient
        serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(OpenAIClient)), Times.Once);

        // Verify that GetService was called for ILoggerFactory
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
    }
}
