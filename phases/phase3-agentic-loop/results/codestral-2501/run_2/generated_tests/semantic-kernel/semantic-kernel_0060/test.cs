using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Net.Http;
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

            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(AzureOpenAIClient))).Returns(mockAzureOpenAIClient.Object);

            var kernelBuilder = new KernelBuilder();
            var deploymentName = "testDeployment";
            var serviceId = "testServiceId";
            var modelId = "testModelId";

            // Act
            kernelBuilder.AddAzureOpenAIAudioToText(deploymentName, null, serviceId, modelId);

            // Assert
            var service = kernelBuilder.Services.BuildServiceProvider().GetService<IAudioToTextService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_ShouldUseProvidedAzureOpenAIClient()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockAzureOpenAIClient = new Mock<AzureOpenAIClient>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            var kernelBuilder = new KernelBuilder();
            var deploymentName = "testDeployment";
            var serviceId = "testServiceId";
            var modelId = "testModelId";

            // Act
            kernelBuilder.AddAzureOpenAIAudioToText(deploymentName, mockAzureOpenAIClient.Object, serviceId, modelId);

            // Assert
            var service = kernelBuilder.Services.BuildServiceProvider().GetService<IAudioToTextService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_ShouldThrowExceptionWhenDeploymentNameIsNull()
        {
            // Arrange
            var kernelBuilder = new KernelBuilder();
            string deploymentName = null;
            var serviceId = "testServiceId";
            var modelId = "testModelId";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => kernelBuilder.AddAzureOpenAIAudioToText(deploymentName, null, serviceId, modelId));
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_ShouldThrowExceptionWhenDeploymentNameIsEmpty()
        {
            // Arrange
            var kernelBuilder = new KernelBuilder();
            var deploymentName = "";
            var serviceId = "testServiceId";
            var modelId = "testModelId";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => kernelBuilder.AddAzureOpenAIAudioToText(deploymentName, null, serviceId, modelId));
        }
    }
}
