using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddOpenAIChatClient("modelId", new Uri("https://example.com"), "apiKey", "orgId", "serviceId", null, null, null);

            // Assert
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }
    }
}
