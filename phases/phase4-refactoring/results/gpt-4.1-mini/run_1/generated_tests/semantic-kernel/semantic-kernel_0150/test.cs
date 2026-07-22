using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Extensions;
using Moq;
using Xunit;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaChatCompletion_WithUri_RegistersServiceAndCallsGetService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOllamaChatCompletion("modelId", new Uri("http://localhost"));

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var service = serviceProvider.GetService<IChatCompletionService>();
        Assert.NotNull(service);
    }
}
