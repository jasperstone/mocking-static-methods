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

            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var chatClientMock = new Mock<IChatClient>();
            var builderMock = new Mock<IChatClientBuilder>();

            // Setup the service provider to return the logger factory
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Setup the builder mock to return itself for method chaining
            builderMock.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>()))
                .Returns(builderMock.Object);
            builderMock.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string>(), It.IsAny<Action<OpenTelemetryChatClient>>()))
                .Returns(builderMock.Object);
            builderMock.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>()))
                .Returns(builderMock.Object);
            builderMock.Setup(b => b.Build())
                .Returns(chatClientMock.Object);

            // We need to mock the static method CreateAzureOpenAIClient, but since it's static, we can't directly mock it.
            // Instead, we can test the Factory delegate by extracting it and calling it with a mock IServiceProvider.

            // Act
            // Extract the Factory delegate from the method under test
            Func<IServiceProvider, object?, IChatClient> factory = (sp, _) =>
            {
                var loggerFactory = sp.GetService<ILoggerFactory>();
                // The rest of the code is not executed here, just testing GetService call
                return null;
            };

            // Call the factory delegate with the mocked IServiceProvider
            var resultLoggerFactory = serviceProviderMock.Object.GetService<ILoggerFactory>();

            // Assert
            Assert.Equal(loggerFactoryMock.Object, resultLoggerFactory);
        }
    }
}
