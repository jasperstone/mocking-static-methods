using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_GetService_CalledOnce()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(p => p.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Act
            services.AddAzureOpenAIChatClient(
                "deploymentName",
                "endpoint",
                "apiKey",
                serviceId: null,
                modelId: null,
                apiVersion: null,
                httpClient: null,
                openTelemetrySourceName: null,
                openTelemetryConfig: null);

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
