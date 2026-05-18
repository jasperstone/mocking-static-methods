using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.AzureOpenAI.Tests.Extensions;

public class AzureOpenAIAudioToTextTests
{
    [Fact]
    public void AddAzureOpenAIAudioToTextService_WithClient_Succeeds()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act
        var result = builder.AddAzureOpenAIAudioToText("test-deployment", openAIClient: null);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddAzureOpenAIAudioToTextService_WithoutClient_Succeeds()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act
        var result = builder.AddAzureOpenAIAudioToText("test-deployment");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddAzureOpenAIAudioToTextService_WithServiceId_Succeeds()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act
        var result = builder.AddAzureOpenAIAudioToText("test-deployment", serviceId: "test-service");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddAzureOpenAIAudioToTextService_WithModelId_Succeeds()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act
        var result = builder.AddAzureOpenAIAudioToText("test-deployment", modelId: "test-model");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddAzureOpenAIAudioToTextService_NullBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IKernelBuilder)null!).AddAzureOpenAIAudioToText("test"));
    }

    [Fact]
    public void AddAzureOpenAIAudioToTextService_EmptyDeploymentName_ThrowsArgumentException()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => builder.AddAzureOpenAIAudioToText(""));
        Assert.ThrowsAny<ArgumentException>(() => builder.AddAzureOpenAIAudioToText("   "));
        Assert.ThrowsAny<ArgumentException>(() => builder.AddAzureOpenAIAudioToText(null!));
    }
}
