using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net.Http;

namespace OpenAIServiceCollectionExtensionsTests
{
    [TestClass]
    public class OpenAIServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void AddOpenAIChatClient_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);

            // Act
            var result = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(loggerFactoryMock.Object, result);
        }

        [TestMethod]
        public void AddOpenAIChatClient_GetService_ReturnsNullWhenNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void AddOpenAIChatClient_GetService_ThrowsExceptionWhenNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.ThrowsException<InvalidOperationException>(() => serviceProvider.GetRequiredService<ILoggerFactory>());
        }

        [TestMethod]
        public void AddOpenAIChatClient_CustomEndpoint_ReturnsHttpClientWithBaseAddress()
        {
            // Arrange
            var services = new ServiceCollection();
            var endpoint = new Uri("https://example.com");
            var httpClient = new HttpClient();
            services.AddSingleton<HttpClient>(httpClient);

            // Act
            services.AddOpenAIChatClient("modelId", endpoint, "apiKey", "orgId", "serviceId", httpClient);
            var serviceProvider = services.BuildServiceProvider();
            var result = serviceProvider.GetService<HttpClient>();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(endpoint, result.BaseAddress);
        }
    }
}
