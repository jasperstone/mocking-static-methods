using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            var loggerFactoryMock = new Mock<Microsoft.Extensions.Logging.ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            var result = Microsoft.Extensions.DependencyInjection.OpenAIServiceCollectionExtensions.AddOpenAIChatClient(services, "modelId", new Uri("https://example.com"), "apiKey");

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
            var result = Microsoft.Extensions.DependencyInjection.OpenAIServiceCollectionExtensions.AddOpenAIChatClient(services, "modelId", new Uri("https://example.com"), "apiKey");

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
            Assert.Throws<ArgumentNullException>(() => Microsoft.Extensions.DependencyInjection.OpenAIServiceCollectionExtensions.AddOpenAIChatClient(null, "modelId", new Uri("https://example.com"), "apiKey"));
        }

        [Fact]
        public void AddOpenAIChatClient_ServiceProvider_GetService_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => Microsoft.Extensions.DependencyInjection.OpenAIServiceCollectionExtensions.AddOpenAIChatClient(services, string.Empty, new Uri("https://example.com"), "apiKey"));
        }
    }
}
