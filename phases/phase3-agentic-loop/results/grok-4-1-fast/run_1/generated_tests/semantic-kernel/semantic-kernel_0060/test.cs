using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Xunit;
using Azure.AI.OpenAI;

namespace Microsoft.SemanticKernel.Connectors.AzureOpenAI.Test;

public class AzureOpenAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddAzureOpenAIAudioToText_WithClientAndLoggerFactory_ResolvesServiceCorrectly()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        var loggerFactory = Mock.Of<ILoggerFactory>();
        var mockClient = new Mock<AzureOpenAIClient>().Object;
        builder.Services.AddSingleton(loggerFactory);
        builder.Services.AddSingleton(mockClient);

        builder.AddAzureOpenAIAudioToText("test-deployment", mockClient, modelId: "test-model");

        // Act
        var kernel = builder.Build();
        var audioService = kernel.GetRequiredService<IAudioToTextService>();

        // Assert
        Assert.IsType<AzureOpenAIAudioToTextService>(audioService);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_WithClientNoLogger_ResolvesServiceCorrectly()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        var mockClient = new Mock<AzureOpenAIClient>().Object;
        builder.Services.AddSingleton(mockClient);

        builder.AddAzureOpenAIAudioToText("test-deployment", mockClient);

        // Act
        var kernel = builder.Build();
        var audioService = kernel.GetRequiredService<IAudioToTextService>();

        // Assert
        Assert.IsType<AzureOpenAIAudioToTextService>(audioService);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_NoClient_ResolvesServiceUsingServiceProvider()
    {
        // Arrange
        var mockClient = new Mock<AzureOpenAIClient>().Object;
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(mockClient);

        builder.AddAzureOpenAIAudioToText("test-deployment");

        // Act
        var kernel = builder.Build();
        var audioService = kernel.GetRequiredService<IAudioToTextService>();

        // Assert
        Assert.IsType<AzureOpenAIAudioToTextService>(audioService);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_VerifiesNotNullBuilder()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IKernelBuilder?)null)!.AddAzureOpenAIAudioToText("test-deployment"));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_VerifiesNotNullOrWhiteSpaceDeploymentName()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => builder.AddAzureOpenAIAudioToText(""));
        Assert.ThrowsAny<ArgumentException>(() => builder.AddAzureOpenAIAudioToText("   "));
        Assert.Throws<ArgumentNullException>(() => builder.AddAzureOpenAIAudioToText(null!));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_KeyedService_ResolvesWithServiceId()
    {
        // Arrange
        var mockClient = new Mock<AzureOpenAIClient>().Object;
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(mockClient);

        builder.AddAzureOpenAIAudioToText("test-deployment", mockClient, serviceId: "test-key");

        // Act
        var kernel = builder.Build();
        var serviceProvider = kernel.Services;
        var keyedService = serviceProvider.GetKeyedService<IAudioToTextService>("test-key")!;

        // Assert
        Assert.NotNull(keyedService);
        Assert.IsType<AzureOpenAIAudioToTextService>(keyedService);
    }

    [Fact]
    public void AddAzureOpenAIAudioToTextService_WithOverloadClient_UsesGetServiceForLoggerFactory()
    {
        // Arrange - Tests the specific factory lambda using serviceProvider.GetService<ILoggerFactory>()
        var builder = Kernel.CreateBuilder();
        var loggerFactory = Mock.Of<ILoggerFactory>();
        var mockClient = new Mock<AzureOpenAIClient>().Object;
        builder.Services.AddSingleton(loggerFactory);
        builder.Services.AddSingleton(mockClient);

        // This tests the overload with deploymentName, client, serviceId, modelId
        // which uses: new(deploymentName, client, modelId, serviceProvider.GetService<ILoggerFactory>());
        builder.AddAzureOpenAIAudioToText("test-deployment", mockClient, serviceId: "test-service", modelId: "test-model");

        // Act
        var kernel = builder.Build();
        var service = kernel.Services.GetKeyedService<IAudioToTextService>("test-service")!;

        // Assert - Service creation succeeds when ILoggerFactory is available via GetService
        Assert.NotNull(service);
        Assert.IsType<AzureOpenAIAudioToTextService>(service);
    }

    [Fact]
    public void AddAzureOpenAIAudioToTextService_NoClientUsesGetRequiredService_ResolvesCorrectly()
    {
        // Arrange - Tests the overload with no client (uses GetRequiredService<AzureOpenAIClient>)
        var mockClient = new Mock<AzureOpenAIClient>().Object;
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(mockClient);

        builder.AddAzureOpenAIAudioToText("test-deployment");

        // Act
        var kernel = builder.Build();
        var service = kernel.GetRequiredService<IAudioToTextService>();

        // Assert
        Assert.IsType<AzureOpenAIAudioToTextService>(service);
    }
}
