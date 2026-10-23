using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Azure.Identity;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_ServiceProvider_GetService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var builder = new Mock<IKernelBuilder>();
            var deploymentName = "deploymentName";
            var openAIClient = new AzureOpenAIClient(new Uri("https://example.com"), new DefaultAzureCredential(), new AzureOpenAIClientOptions());
            var serviceId = "serviceId";
            var modelId = "modelId";

            // Act
            AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(builder.Object, deploymentName, openAIClient, serviceId, modelId);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
