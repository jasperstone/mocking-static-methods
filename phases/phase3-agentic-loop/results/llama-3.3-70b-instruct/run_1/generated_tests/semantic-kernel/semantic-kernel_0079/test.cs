using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_ServiceProvider_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            AzureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(
                services,
                "deploymentName",
                "endpoint",
                "apiKey",
                "serviceId",
                "modelId",
                "apiVersion",
                null,
                "openTelemetrySourceName",
                null);

            // Assert
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }
    }
}
