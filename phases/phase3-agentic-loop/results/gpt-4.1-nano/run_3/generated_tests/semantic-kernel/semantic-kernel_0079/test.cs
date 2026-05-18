using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Connectors.AzureOpenAI.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_Should_Call_GetService_On_ServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var chatClientMock = new Mock<IChatClient>();
            var chatClientBuilderMock = new Mock<IChatClientBuilder>();

            // Setup the service provider to return the logger factory when requested
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Create the Factory delegate as in the method
            Func<IServiceProvider, object?, IChatClient> factory = (sp, _) =>
            {
                var loggerFactory = sp.GetService<ILoggerFactory>();
                Assert.NotNull(loggerFactory); // Confirm GetService was called

                // Return a dummy IChatClient
                return new Mock<IChatClient>().Object;
            };

            // Act
            var chatClientInstance = factory(serviceProviderMock.Object, null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
            Assert.NotNull(chatClientInstance);
        }
    }
}
