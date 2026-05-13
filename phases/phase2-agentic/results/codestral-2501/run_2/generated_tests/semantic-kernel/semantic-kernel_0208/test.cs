using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_ShouldCallGetServiceAndAddKeyedSingleton()
        {
            // Arrange
            var serviceCollectionMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var modelId = "test-model";
            var apiKey = "test-api-key";
            var endpoint = new Uri("https://api.openai.com/v1");
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            OpenAIServiceCollectionExtensions.AddOpenAIChatClient(
                serviceCollectionMock.Object,
                modelId,
                endpoint,
                apiKey,
                serviceId: serviceId,
                httpClient: httpClient);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
            serviceCollectionMock.Verify(sc => sc.Add(It.IsAny<ServiceDescriptor>()), Times.Once);
        }
    }
}
