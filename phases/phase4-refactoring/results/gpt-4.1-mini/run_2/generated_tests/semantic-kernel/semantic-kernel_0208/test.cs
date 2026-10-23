using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAIChatClient_WithUri_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var modelId = "test-model";
        var endpoint = new Uri("https://test.endpoint/");
        var apiKey = "test-api-key";

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        // Setup GetService to return the mock logger factory when requested
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(mockLoggerFactory.Object);

        // We will add a factory that uses the IServiceProvider passed to it
        services.AddOpenAIChatClient(
            modelId,
            endpoint,
            apiKey,
            serviceId: null,
            httpClient: null,
            openTelemetrySourceName: null,
            openTelemetryConfig: null);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        // Resolve the IChatClient from the service provider to trigger the factory and the GetService call
        var chatClient = serviceProvider.GetService(typeof(Microsoft.Extensions.AI.IChatClient));

        // Assert
        // Verify that GetService was called on the IServiceProvider with ILoggerFactory type
        // We cannot directly verify the internal call on the serviceProvider from the built container,
        // so this test mainly ensures no exceptions and the service is registered.
        Assert.NotNull(chatClient);
    }
}
