using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.HuggingFace;

namespace Connectors.HuggingFace.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_ModelEndpointApiKeyServiceIdHttpClient_ServiceProviderGetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var model = "model";
            var endpoint = new Uri("https://example.com");
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            services.AddHuggingFaceImageToText(model, endpoint, apiKey, serviceId, httpClient);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }

        [Fact]
        public void AddHuggingFaceImageToText_EndpointApiKeyServiceIdHttpClient_ServiceProviderGetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var endpoint = new Uri("https://example.com");
            var apiKey = "apiKey";
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            services.AddHuggingFaceImageToText(endpoint, apiKey, serviceId, httpClient);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
