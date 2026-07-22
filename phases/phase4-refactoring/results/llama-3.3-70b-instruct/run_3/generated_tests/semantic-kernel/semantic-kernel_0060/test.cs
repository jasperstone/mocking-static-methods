using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Azure.AI.OpenAI;
using Moq;

namespace AzureOpenAIKernelBuilderExtensionsTests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_ServiceProviderGetServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<Microsoft.Extensions.Logging.ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var builder = new Microsoft.SemanticKernel.KernelBuilder();
            var deploymentName = "test-deployment";
            var openAIClient = new AzureOpenAIClient(new Uri("https://example.com"), new Azure.Core.ApiKeyCredential("test-api-key"));
            var serviceId = "test-service-id";
            var modelId = "test-model-id";

            // Act
            builder.AddAzureOpenAIAudioToText(deploymentName, openAIClient, serviceId, modelId);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)), Times.Once);
        }
    }
}
