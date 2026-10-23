using System;
using System.Net.Http;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_ShouldRegisterService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var azureOpenAIClientMock = new Mock<AzureOpenAIClient>(new Uri("https://example.com"), new ApiKeyCredential("test"));

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(AzureOpenAIClient))).Returns(azureOpenAIClientMock.Object);

            var serviceCollection = new ServiceCollection();
            var kernelBuilder = new KernelBuilder(serviceCollection, serviceProviderMock.Object);

            // Act
            kernelBuilder.AddAzureOpenAIAudioToText("testDeployment", null, "testServiceId", "testModelId");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var audioToTextService = serviceProvider.GetService<IAudioToTextService>();

            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);
        }
    }
}
