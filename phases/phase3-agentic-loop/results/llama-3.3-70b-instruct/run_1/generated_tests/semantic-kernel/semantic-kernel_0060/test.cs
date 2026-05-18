using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Azure;
using Microsoft.SemanticKernel.AudioToText;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_CreatesAzureOpenAIAudioToTextService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .BuildServiceProvider();

            var builder = new KernelBuilder();
            var deploymentName = "test-deployment";
            var openAIClient = new AzureOpenAIClient(new Uri("https://test-endpoint"), new DefaultAzureCredential());
            var modelId = "test-model";

            // Act
            builder.AddAzureOpenAIAudioToText(deploymentName, openAIClient, modelId: modelId);

            // Assert
            var audioToTextService = serviceProvider.GetService<IAudioToTextService>();
            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_GetService_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            var builder = new KernelBuilder();
            var deploymentName = "test-deployment";
            var openAIClient = new AzureOpenAIClient(new Uri("https://test-endpoint"), new DefaultAzureCredential());
            var modelId = "test-model";

            // Act
            builder.AddAzureOpenAIAudioToText(deploymentName, openAIClient, modelId: modelId);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
