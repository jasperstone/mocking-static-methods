using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_ServiceProvider_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = new Mock<Microsoft.Extensions.Logging.ILoggerFactory>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory))).Returns(loggerFactory);

            // Act
            var result = OpenAIServiceCollectionExtensions.AddOpenAIChatClient(services, "modelId", new Uri("https://example.com"), "apiKey");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void AddOpenAIChatClient_ServiceProvider_GetService_ReturnsNullLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            var result = OpenAIServiceCollectionExtensions.AddOpenAIChatClient(services, "modelId", new Uri("https://example.com"), "apiKey");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void AddOpenAIChatClient_ServiceProvider_GetService_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => OpenAIServiceCollectionExtensions.AddOpenAIChatClient(null, "modelId", new Uri("https://example.com"), "apiKey"));
        }

        [Fact]
        public void AddOpenAIChatClient_ServiceProvider_GetService_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => OpenAIServiceCollectionExtensions.AddOpenAIChatClient(services, null, new Uri("https://example.com"), "apiKey"));
        }
    }
}
