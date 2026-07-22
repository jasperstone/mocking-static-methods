using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Tests.Connectors.AzureOpenAI.Extensions
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_Factory_UsesProvidedOpenAIClient()
        {
            // Arrange
            var deploymentName = "deployment1";
            var modelId = "model1";

            var openAIClientMock = new Mock<AzureOpenAIClient>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            var service = new AzureOpenAIAudioToTextService(
                deploymentName,
                openAIClientMock.Object,
                modelId,
                loggerFactoryMock.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_Factory_UsesServiceProviderOpenAIClient_WhenNotProvided()
        {
            // Arrange
            var deploymentName = "deployment2";
            var modelId = "model2";

            var openAIClientMock = new Mock<AzureOpenAIClient>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(AzureOpenAIClient))).Returns(openAIClientMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            var client = (AzureOpenAIClient)serviceProviderMock.Object.GetService(typeof(AzureOpenAIClient))!;
            var loggerFactory = (ILoggerFactory?)serviceProviderMock.Object.GetService(typeof(ILoggerFactory));
            var service = new AzureOpenAIAudioToTextService(
                deploymentName,
                client,
                modelId,
                loggerFactory);

            // Assert
            Assert.NotNull(service);
        }
    }
}
