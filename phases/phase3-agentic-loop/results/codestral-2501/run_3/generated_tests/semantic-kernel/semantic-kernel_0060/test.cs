using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AudioToText;

namespace AzureOpenAIKernelBuilderExtensionsTests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_ShouldAddServiceToKernelBuilder()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockAzureOpenAIClient = new Mock<AzureOpenAIClient>();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(AzureOpenAIClient)))
                .Returns(mockAzureOpenAIClient.Object);

            var kernelBuilder = Kernel.Builder;

            // Act
            kernelBuilder.AddAzureOpenAIAudioToText("deploymentName", null, "serviceId", "modelId");

            // Assert
            var serviceProvider = kernelBuilder.Services.BuildServiceProvider();
            var audioToTextService = serviceProvider.GetService<IAudioToTextService>();

            Assert.NotNull(audioToTextService);
        }
    }
}
