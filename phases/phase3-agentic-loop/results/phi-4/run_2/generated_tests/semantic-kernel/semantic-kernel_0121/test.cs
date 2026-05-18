using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            var model = "test-model";
            var endpoint = new Uri("http://example.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            HuggingFaceServiceCollectionExtensions.AddHuggingFaceImageToText(
                services,
                model,
                endpoint,
                apiKey,
                serviceId,
                httpClient
            );

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }

        [Fact]
        public void AddHuggingFaceImageToText_InitializesHuggingFaceImageToTextServiceCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var httpClientProviderMock = new Mock<HttpClientProvider>();
            var httpClientMock = new Mock<HttpClient>();

            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);
            httpClientProviderMock.Setup(provider => provider.GetHttpClient(It.IsAny<HttpClient>(), It.IsAny<IServiceProvider>()))
                .Returns(httpClientMock.Object);

            var model = "test-model";
            var endpoint = new Uri("http://example.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";

            // Act
            HuggingFaceServiceCollectionExtensions.AddHuggingFaceImageToText(
                services,
                model,
                endpoint,
                apiKey,
                serviceId
            );

            // Assert
            var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IImageToTextService));
            Assert.NotNull(serviceDescriptor);

            var factory = serviceDescriptor.ImplementationFactory;
            Assert.NotNull(factory);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            var service = factory(serviceProvider.Object, null);
            Assert.IsType<HuggingFaceImageToTextService>(service);
            Assert.Equal(model, ((HuggingFaceImageToTextService)service)._client.ModelId);
            Assert.Equal(endpoint, ((HuggingFaceImageToTextService)service)._client.Endpoint);
            Assert.Equal(apiKey, ((HuggingFaceImageToTextService)service)._client.ApiKey);
            Assert.Same(httpClientMock.Object, ((HuggingFaceImageToTextService)service)._client.HttpClient);
        }
    }
}
