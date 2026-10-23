using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;

public class AzureOpenAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddAzureOpenAIAudioToText_ServiceProvider_GetService_Called()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        serviceProviderMock.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

        var builder = new Microsoft.SemanticKernel.KernelBuilder();
        var deploymentName = "test-deployment";
        var openAIClient = new AzureOpenAIClient(new Uri("https://test-endpoint"), new Azure.Core.TokenCredential());
        var modelId = "test-model";

        // Act
        builder.AddAzureOpenAIAudioToText(deploymentName, openAIClient, modelId);

        // Assert
        serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
    }
}
