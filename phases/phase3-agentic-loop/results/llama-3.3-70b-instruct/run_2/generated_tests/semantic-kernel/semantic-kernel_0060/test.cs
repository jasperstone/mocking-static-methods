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
    public void AddAzureOpenAIAudioToText_ServiceProvider_GetService_Called()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

        var builder = new KernelBuilder();
        var deploymentName = "deploymentName";
        var openAIClient = new AzureOpenAIClient(new Uri("https://example.com"), new ApiKeyCredential("apiKey"));
        var serviceId = "serviceId";
        var modelId = "modelId";

        // Act
        builder.AddAzureOpenAIAudioToText(deploymentName, openAIClient, serviceId, modelId);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
    }

    [Fact]
    public void AddAzureOpenAIAudioToText_ServiceProvider_GetRequiredService_Called()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var azureOpenAIClientMock = new Mock<AzureOpenAIClient>();
        serviceProviderMock.Setup(sp => sp.GetRequiredService<AzureOpenAIClient>()).Returns(azureOpenAIClientMock.Object);
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

        var builder = new KernelBuilder();
        var deploymentName = "deploymentName";
        var serviceId = "serviceId";
        var modelId = "modelId";

        // Act
        builder.AddAzureOpenAIAudioToText(deploymentName, serviceId: serviceId, modelId: modelId);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<AzureOpenAIClient>(), Times.Once);
    }
}
