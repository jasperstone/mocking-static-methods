using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_Should_Call_GetService_And_Return_Service()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var chatClientMock = new Mock<IChatClient>();

            // Setup IServiceProvider to return ILoggerFactory
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Setup IServiceProvider to return IChatClient
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IChatClient)))
                .Returns(chatClientMock.Object);

            // Act
            var result = OpenAIServiceCollectionExtensions.AddOpenAIChatClient(
                services,
                "model-id",
                "api-key",
                orgId: null,
                serviceId: "service-id",
                httpClient: null,
                openTelemetrySourceName: null,
                openTelemetryConfig: null);

            // Assert
            Assert.Same(services, result);
        }
    }
}
