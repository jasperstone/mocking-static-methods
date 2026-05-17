using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_Should_Call_GetService_On_ServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var openAIClientMock = new Mock<OpenAI.OpenAIClient>();

            // Setup GetService to return loggerFactoryMock when ILoggerFactory is requested
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Setup GetRequiredService to return openAIClientMock when OpenAIClient is requested
            var serviceProvider = serviceProviderMock.Object;

            // Act
            services.AddOpenAIChatClient(
                "model-id",
                apiKey: "test-api-key",
                orgId: null,
                serviceId: "service-id",
                openAIClient: openAIClientMock.Object);

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Resolve the IChatClient to trigger the Factory method
            var chatClient = provider.GetService<IChatClient>();

            // Assert
            // Verify that GetService was called on the service provider for ILoggerFactory
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
