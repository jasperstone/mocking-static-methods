using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.ImageToText;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.UnitTests;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceImageToText_WithModel_RegistersKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddHuggingFaceImageToText("test-model");

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IImageToTextService)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithEndpoint_RegistersKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddHuggingFaceImageToText(new Uri("https://example.com"));

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IImageToTextService)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithModel_CreatesServiceWithLoggerFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddHuggingFaceImageToText("test-model");
        using var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetRequiredService<IImageToTextService>();

        // Assert
        Assert.IsType<HuggingFaceImageToTextService>(imageService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithEndpoint_CreatesServiceWithLoggerFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddHuggingFaceImageToText(new Uri("https://example.com"));
        using var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetRequiredService<IImageToTextService>();

        // Assert
        Assert.IsType<HuggingFaceImageToTextService>(imageService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddHuggingFaceImageToText("test-model"));
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddHuggingFaceImageToText(new Uri("https://example.com")));
    }

    [Fact]
    public void AddHuggingFaceTextGeneration_WithModel_RegistersKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddHuggingFaceTextGeneration("test-model");

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(Microsoft.SemanticKernel.TextGeneration.ITextGenerationService)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddHuggingFaceChatCompletion_WithModel_RegistersKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddHuggingFaceChatCompletion("test-model");

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }
}
