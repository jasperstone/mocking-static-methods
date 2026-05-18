using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.AzureOpenAI.Tests;

public class AzureOpenAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddAzureOpenAIAudioToText_ServiceProviderHasAzureOpenAIClient_ReturnsAzureOpenAIAudioToTextService()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<AzureOpenAIClient>(new AzureOpenAIClient(new Uri("https://example.com"), new ApiKeyCredential("api-key")))
            .AddSingleton<ILoggerFactory>(new LoggerFactory())
            .BuildServiceProvider();

        var builder = new KernelBuilder();
        var deploymentName = "deployment-name";
        var serviceId = "service-id";
        var modelId = "model-id";

        // Act
        builder.AddAzureOpenAIAudioToText(deploymentName, serviceId: serviceId, modelId: modelId);

        // Assert
        var audioToTextService = serviceProvider.GetService<IAudioToTextService>();
        Assert.NotNull(audioToTextService);
        Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_ServiceProviderDoesNotHaveAzureOpenAIClient_ThrowsException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(new LoggerFactory())
            .BuildServiceProvider();

        var builder = new KernelBuilder();
        var deploymentName = "deployment-name";
        var serviceId = "service-id";
        var modelId = "model-id";

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => builder.AddAzureOpenAIAudioToText(deploymentName, serviceId: serviceId, modelId: modelId));
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_LoggerFactoryIsNull_DoesNotThrowException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<AzureOpenAIClient>(new AzureOpenAIClient(new Uri("https://example.com"), new ApiKeyCredential("api-key")))
            .BuildServiceProvider();

        var builder = new KernelBuilder();
        var deploymentName = "deployment-name";
        var serviceId = "service-id";
        var modelId = "model-id";

        // Act
        builder.AddAzureOpenAIAudioToText(deploymentName, serviceId: serviceId, modelId: modelId);

        // Assert
        var audioToTextService = serviceProvider.GetService<IAudioToTextService>();
        Assert.NotNull(audioToTextService);
        Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);
    }
}
