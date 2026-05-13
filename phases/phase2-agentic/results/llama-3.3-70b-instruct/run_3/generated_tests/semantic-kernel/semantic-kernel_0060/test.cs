using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Xunit;

namespace Tests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_ServiceProviderGetService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            var builder = new KernelBuilder();
            builder.Services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            builder.AddAzureOpenAIAudioToText("deploymentName", null, "serviceId", "modelId");

            // Assert
            serviceProviderMock.Verify(p => p.GetService<ILoggerFactory>(), Times.Once);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_ServiceProviderGetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var azureOpenAIClientMock = new Mock<AzureOpenAIClient>();
            serviceProviderMock.Setup(p => p.GetRequiredService<AzureOpenAIClient>()).Returns(azureOpenAIClientMock.Object);

            var builder = new KernelBuilder();
            builder.Services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            builder.AddAzureOpenAIAudioToText("deploymentName", null, "serviceId", "modelId");

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<AzureOpenAIClient>(), Times.Once);
        }
    }
}
