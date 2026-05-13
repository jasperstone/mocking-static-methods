using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        private const string TestModelId = "test-model";

        [Fact]
        public void AddOpenAIChatClient_WithServiceProvider_CallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var httpClientMock = new HttpClient();

            // Setup GetService to return loggerFactoryMock when ILoggerFactory is requested
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Act
            services.AddSingleton(serviceProviderMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Call the method under test
            var result = OpenAIServiceCollectionExtensions.AddOpenAIChatClient(
                services,
                TestModelId,
                "test-api-key",
                orgId: null,
                serviceId: "service1",
                httpClient: null,
                openTelemetrySourceName: null,
                openTelemetryConfig: null);

            // Assert
            Assert.Same(services, result);
            // Verify that GetService was called on the service provider
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
