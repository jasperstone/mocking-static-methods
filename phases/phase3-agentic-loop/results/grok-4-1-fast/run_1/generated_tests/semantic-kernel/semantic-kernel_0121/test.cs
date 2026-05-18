using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.ImageToText;
using Xunit;

namespace Microsoft.SemanticKernel.Test.Connectors.HuggingFace;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceImageToText_WithModel_SucceedsAndRegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        // Act
        var result = services.AddHuggingFaceImageToText("test-model");

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>(null);
        Assert.NotNull(imageService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithModelAndEndpoint_SucceedsAndRegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        var endpoint = new Uri("https://example.com");

        // Act
        var result = services.AddHuggingFaceImageToText("test-model", endpoint);

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>(null);
        Assert.NotNull(imageService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_OnlyEndpoint_SucceedsAndRegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        var endpoint = new Uri("https://example.com");

        // Act
        var result = services.AddHuggingFaceImageToText(endpoint);

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>(null);
        Assert.NotNull(imageService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithCustomServiceId_RegistersWithCorrectKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        // Act
        services.AddHuggingFaceImageToText("test-model", serviceId: "custom-id");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>("custom-id");
        Assert.NotNull(imageService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithNoLoggerFactory_HandlesNullGetServiceResult()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should not throw when GetService<ILoggerFactory>() returns null
        var result = services.AddHuggingFaceImageToText("test-model");
        Assert.Same(services, result);

        // Verify service can be resolved (service registration succeeds)
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>(null);
        Assert.NotNull(imageService);
    }
}
