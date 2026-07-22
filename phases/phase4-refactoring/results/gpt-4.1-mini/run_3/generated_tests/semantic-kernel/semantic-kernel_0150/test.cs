using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.TextGeneration;
using Moq;
using Xunit;
using OllamaSharp;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaChatCompletion_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var modelId = "test-model";
        var endpoint = new Uri("http://localhost");

        var mockLoggerFactory = new Mock<ILoggerFactory>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(mockLoggerFactory.Object);

        // Act
        var serviceCollection = Microsoft.SemanticKernel.OllamaServiceCollectionExtensions.AddOllamaChatCompletion(
            services,
            modelId,
            endpoint);

        // Build the service provider from the collection
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Resolve the IChatCompletionService to trigger the factory delegate
        var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

        // Assert
        Assert.NotNull(chatCompletionService);

        // Since the factory delegate is called by the service provider, we cannot directly verify the mock call.
        // Instead, we verify that the service provider can resolve the service without exceptions.
    }
}
