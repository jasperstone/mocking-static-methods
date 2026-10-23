using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.ImageToText;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.UnitTests;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceImageToText_WithModel_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHuggingFaceImageToText("test-model");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>(null);
        Assert.NotNull(imageService);
        Assert.IsType<HuggingFaceImageToTextService>(imageService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithEndpoint_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHuggingFaceImageToText(new Uri("https://test-endpoint.com"));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>(null);
        Assert.NotNull(imageService);
        Assert.IsType<HuggingFaceImageToTextService>(imageService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        const string serviceId = "test-service";

        // Act
        services.AddHuggingFaceImageToText("test-model", serviceId: serviceId);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>(serviceId);
        Assert.NotNull(imageService);
        Assert.IsType<HuggingFaceImageToTextService>(imageService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithLoggerFactoryAvailable_UsesResolvedLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedLoggerFactory = NullLoggerFactory.Instance;
        services.AddSingleton<ILoggerFactory>(expectedLoggerFactory);

        // Act
        services.AddHuggingFaceImageToText("test-model");

        // Assert - Building triggers factory execution which calls serviceProvider.GetService<ILoggerFactory>()
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>(null);
        Assert.NotNull(imageService);
        Assert.IsType<HuggingFaceImageToTextService>(imageService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_NoLoggerFactoryAvailable_StillRegistersService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHuggingFaceImageToText("test-model");

        // Assert - serviceProvider.GetService<ILoggerFactory>() returns null, but service still created
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>(null);
        Assert.NotNull(imageService);
        Assert.IsType<HuggingFaceImageToTextService>(imageService);
    }
}
