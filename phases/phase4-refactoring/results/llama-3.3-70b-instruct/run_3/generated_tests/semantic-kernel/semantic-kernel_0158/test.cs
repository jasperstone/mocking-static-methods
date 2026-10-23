using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using OllamaSharp;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetService_OllamaApiClient_ReturnsOllamaApiClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<OllamaApiClient>(provider => new OllamaApiClient("https://example.com", "modelId"));
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var ollamaClient = serviceProvider.GetService<OllamaApiClient>();

        // Assert
        Assert.NotNull(ollamaClient);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetService_IOllamaApiClient_ReturnsOllamaApiClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IOllamaApiClient>(provider => new OllamaApiClient("https://example.com", "modelId"));
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var ollamaClient = serviceProvider.GetService<IOllamaApiClient>() as OllamaApiClient;

        // Assert
        Assert.NotNull(ollamaClient);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetRequiredService_IOllamaApiClient_ReturnsOllamaApiClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IOllamaApiClient>(provider => new OllamaApiClient("https://example.com", "modelId"));
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var ollamaClient = serviceProvider.GetRequiredService<IOllamaApiClient>() as OllamaApiClient;

        // Assert
        Assert.NotNull(ollamaClient);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetService_OllamaApiClient_ThrowsInvalidOperationException_WhenNoOllamaApiClientFound()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act and Assert
        Assert.Null(serviceProvider.GetService<OllamaApiClient>());
    }
}
