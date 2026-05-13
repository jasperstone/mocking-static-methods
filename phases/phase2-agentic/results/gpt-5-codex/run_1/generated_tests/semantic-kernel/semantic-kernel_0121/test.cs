using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.ImageToText;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.UnitTests;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceImageToText_ResolvesLoggerFactoryFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        bool loggerFactoryRequested = false;

        services.AddSingleton<ILoggerFactory>(_ =>
        {
            loggerFactoryRequested = true;
            return new LoggerFactory();
        });

        using var httpClient = new HttpClient();

        services.AddHuggingFaceImageToText(
            model: "test-model",
            endpoint: new Uri("https://example.com"),
            apiKey: "test-key",
            serviceId: "image-service",
            httpClient: httpClient);

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var service = serviceProvider.GetRequiredKeyedService<IImageToTextService>("image-service");

        // Assert
        Assert.IsType<HuggingFaceImageToTextService>(service);
        Assert.True(loggerFactoryRequested);
    }

    [Fact]
    public void AddHuggingFaceImageToText_DoesNotRequireLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        using var httpClient = new HttpClient();

        services.AddHuggingFaceImageToText(
            endpoint: new Uri("https://example.com"),
            apiKey: "test-key",
            serviceId: "image-service",
            httpClient: httpClient);

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var service = serviceProvider.GetRequiredKeyedService<IImageToTextService>("image-service");

        // Assert
        Assert.IsType<HuggingFaceImageToTextService>(service);
    }
}
