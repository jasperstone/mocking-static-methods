using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.ImageToText;
using Microsoft.SemanticKernel.TextGeneration;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.UnitTests;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceImageToText_WithModel_RegistersKeyedSingletonAndUsesGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddHuggingFaceImageToText("test-model");

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IImageToTextService)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

        var serviceProvider = services.BuildServiceProvider();
        var instance = serviceProvider.GetRequiredService<IImageToTextService>();
        Assert.IsType<HuggingFaceImageToTextService>(instance);
    }

    [Fact]
    public void AddHuggingFaceImageToText_WithEndpoint_RegistersKeyedSingletonAndUsesGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddHuggingFaceImageToText(new Uri("https://example.com"));

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IImageToTextService)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

        var serviceProvider = services.BuildServiceProvider();
        var instance = serviceProvider.GetRequiredService<IImageToTextService>();
        Assert.IsType<HuggingFaceImageToTextService>(instance);
    }

    [Fact]
    public void AddHuggingFaceImageToText_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddHuggingFaceImageToText("test-model"));
    }

    [Fact]
    public void AddHuggingFaceTextGeneration_WithModel_RegistersKeyedSingletonAndUsesGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddHuggingFaceTextGeneration("test-model");

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextGenerationService)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

        var serviceProvider = services.BuildServiceProvider();
        var instance = serviceProvider.GetRequiredService<ITextGenerationService>();
        Assert.IsType<HuggingFaceTextGenerationService>(instance);
    }

    [Fact]
    public void AddHuggingFaceChatCompletion_WithModel_RegistersKeyedSingletonAndUsesGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddHuggingFaceChatCompletion("test-model");

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IChatCompletionService)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

        var serviceProvider = services.BuildServiceProvider();
        var instance = serviceProvider.GetRequiredService<IChatCompletionService>();
        Assert.IsType<HuggingFaceChatCompletionService>(instance);
    }
}
