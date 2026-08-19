using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_ShouldAddServiceToKernelBuilder()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var azureOpenAIClientMock = new Mock<AzureOpenAIClient>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(AzureOpenAIClient))).Returns(azureOpenAIClientMock.Object);

            var kernelBuilder = new KernelBuilder();
            kernelBuilder.Services.AddSingleton(serviceProviderMock.Object);

            // Act
            kernelBuilder.AddAzureOpenAIAudioToText("deploymentName", null, "serviceId", "modelId");

            // Assert
            var service = kernelBuilder.Services.BuildServiceProvider().GetService<IAudioToTextService>();
            Assert.NotNull(service);
        }
    }
}
