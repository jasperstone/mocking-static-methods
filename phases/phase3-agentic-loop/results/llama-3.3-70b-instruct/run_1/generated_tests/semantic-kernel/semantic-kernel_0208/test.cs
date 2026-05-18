using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_ServiceProvider_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loggerFactoryMock.Object, result);
        }

        [Fact]
        public void AddOpenAIChatClient_ServiceProvider_GetService_ReturnsNull_WhenLoggerFactoryNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void AddOpenAIChatClient_ServiceProvider_GetService_ReturnsHttpClient_WhenHttpClientRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var httpClient = new HttpClient();
            services.AddSingleton<HttpClient>(httpClient);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetService<HttpClient>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(httpClient, result);
        }

        [Fact]
        public void AddOpenAIChatClient_ServiceProvider_GetService_ReturnsNull_WhenHttpClientNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetService<HttpClient>();

            // Assert
            Assert.Null(result);
        }
    }
}
