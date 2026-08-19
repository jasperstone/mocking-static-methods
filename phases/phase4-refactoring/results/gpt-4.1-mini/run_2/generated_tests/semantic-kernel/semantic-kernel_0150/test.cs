using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.TextGeneration;
using Moq;
using OllamaSharp;
using Xunit;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaChatCompletion_UsesGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add a mock ILoggerFactory to the service collection
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var loggerMock = new Mock<ILogger>();
        loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        services.AddSingleton(loggerFactoryMock.Object);

        // Act
        var modelId = "test-model";
        var endpoint = new Uri("http://localhost");

        // Call the extension method
        var updatedServices = services.AddOllamaChatCompletion(modelId, endpoint);

        // Build the service provider to resolve the service and trigger the factory delegate
        var serviceProvider = updatedServices.BuildServiceProvider();

        // Resolve the IChatCompletionService to trigger the factory delegate and the GetService call
        var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

        // Assert
        Assert.NotNull(chatCompletionService);
    }
}
