using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.AzureOpenAI.UnitTests.Extensions;

public class AzureOpenAIAudioToTextTests
{
    [Fact]
    public void AddAzureOpenAIAudioToText_ReturnsSameBuilder()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act
        var result = builder.AddAzureOpenAIAudioToText("test-deployment");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IKernelBuilder?)null)!.AddAzureOpenAIAudioToText("test-deployment"));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_WithEmptyDeploymentName_ThrowsArgumentException()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddAzureOpenAIAudioToText(""));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_WithWhitespaceDeploymentName_ThrowsArgumentException()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddAzureOpenAIAudioToText("   "));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_RegistersServiceWithCorrectKey()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<Azure.AI.OpenAI.AzureOpenAIClient>(new Mock<Azure.AI.OpenAI.AzureOpenAIClient>().Object);

        // Act
        builder.AddAzureOpenAIAudioToText("test-deployment");

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var audioService = serviceProvider.GetKeyedService<IAudioToTextService>("test-deployment");
        Assert.NotNull(audioService);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_WithCustomServiceId_RegistersWithCustomKey()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<Azure.AI.OpenAI.AzureOpenAIClient>(new Mock<Azure.AI.OpenAI.AzureOpenAIClient>().Object);

        const string serviceId = "custom-id";

        // Act
        builder.AddAzureOpenAIAudioToText("test-deployment", serviceId: serviceId);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var audioService = serviceProvider.GetKeyedService<IAudioToTextService>(serviceId);
        Assert.NotNull(audioService);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_UsesProvidedClientWhenAvailable()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        var mockClient = new Mock<Azure.AI.OpenAI.AzureOpenAIClient>();

        // Act
        builder.AddAzureOpenAIAudioToText("test-deployment", openAIClient: mockClient.Object);

        // Assert - Service gets registered
        var serviceProvider = builder.Services.BuildServiceProvider();
        var audioService = serviceProvider.GetKeyedService<IAudioToTextService>("test-deployment");
        Assert.NotNull(audioService);
    }
}
