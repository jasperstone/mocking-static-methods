using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Services;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.AzureOpenAI.Tests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_ServiceProviderGetService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            var builderMock = new Mock<IKernelBuilder>();
            builderMock.SetupGet(b => b.Services).Returns(new ServiceCollection());

            var deploymentName = "deploymentName";
            var openAIClient = new AzureOpenAIClient(new Uri("https://example.com"), new ApiKeyCredential("apiKey"));
            var serviceId = "serviceId";
            var modelId = "modelId";

            // Act
            var result = AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(builderMock.Object, deploymentName, openAIClient, serviceId, modelId);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_ServiceProviderGetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var openAIClientMock = new Mock<AzureOpenAIClient>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<AzureOpenAIClient>()).Returns(openAIClientMock.Object);

            var builderMock = new Mock<IKernelBuilder>();
            builderMock.SetupGet(b => b.Services).Returns(new ServiceCollection());

            var deploymentName = "deploymentName";
            var serviceId = "serviceId";
            var modelId = "modelId";

            // Act
            var result = AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(builderMock.Object, deploymentName, null, serviceId, modelId);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<AzureOpenAIClient>(), Times.Once);
        }
    }
}
