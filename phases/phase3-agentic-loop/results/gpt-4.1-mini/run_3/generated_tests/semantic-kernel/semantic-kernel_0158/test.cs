using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Moq;
using OllamaSharp;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.Ollama.Extensions;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaTextGeneration_UsesGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // We will mock IServiceProvider to verify GetService<T> calls
        var mockServiceProvider = new Mock<IServiceProvider>();

        // Setup GetService<ILoggerFactory> to return null (no logger)
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(null);

        // Setup GetKeyedService extension methods - these are extension methods, so we simulate by adding them to the service provider mock
        // We cannot mock extension methods directly, so we will simulate the fallback chain by setting up GetService calls for OllamaApiClient and IOllamaApiClient

        // Setup GetService<OllamaApiClient> to return null to test fallback to GetRequiredService<IOllamaApiClient>
        mockServiceProvider.Setup(sp => sp.GetService(typeof(OllamaApiClient))).Returns(null);

        // Setup GetRequiredService<IOllamaApiClient> to return a mock OllamaApiClient instance
        var mockOllamaApiClient = new Mock<IOllamaApiClient>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IOllamaApiClient))).Returns(mockOllamaApiClient.Object);

        // We need to simulate GetRequiredService<T> extension method, which calls GetService<T> and throws if null.
        // Since we cannot mock extension methods, we will add the mockServiceProvider to the service collection and use the real extension methods.

        // Add the mockServiceProvider as a singleton to the service collection
        services.AddSingleton(sp => mockServiceProvider.Object);

        // Act
        // Call the extension method under test
        var resultServices = services.AddOllamaTextGeneration(serviceId: "testService");

        // Build the service provider from the collection
        var builtServiceProvider = resultServices.BuildServiceProvider();

        // Resolve the ITextGenerationService to trigger the factory delegate
        var textGenerationService = builtServiceProvider.GetService<ITextGenerationService>();

        // Assert
        Assert.NotNull(textGenerationService);

        // Verify that GetService<ILoggerFactory> was called at least once
        mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);

        // Verify that GetService<OllamaApiClient> was called at least once
        mockServiceProvider.Verify(sp => sp.GetService(typeof(OllamaApiClient)), Times.AtLeastOnce);

        // Verify that GetService<IOllamaApiClient> was called at least once
        mockServiceProvider.Verify(sp => sp.GetService(typeof(IOllamaApiClient)), Times.AtLeastOnce);
    }
}
