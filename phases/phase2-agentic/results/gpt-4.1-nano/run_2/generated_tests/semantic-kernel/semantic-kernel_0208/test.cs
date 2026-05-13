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
        private const string ModelId = "test-model";
        private const string ApiKey = "test-api-key";

        [Fact]
        public void AddOpenAIChatClient_Should_Call_GetService_And_Returns_Service()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var chatClientMock = new Mock<IChatClient>();

            // Setup GetService to return loggerFactoryMock
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Setup GetService for IChatClient to return mock
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IChatClient)))
                .Returns(chatClientMock.Object);

            // Act
            services.AddSingleton(serviceProviderMock.Object);
            var provider = services.BuildServiceProvider();

            // Call extension method
            var result = OpenAIServiceCollectionExtensions.AddOpenAIChatClient(
                services,
                ModelId,
                ApiKey);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ServiceCollection>(result);
        }
    }
}
