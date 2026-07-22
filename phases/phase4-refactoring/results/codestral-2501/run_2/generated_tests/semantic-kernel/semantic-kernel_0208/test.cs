using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_ShouldGetLoggerFactoryFromServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            serviceCollection.AddSingleton<IServiceProvider>(mockServiceProvider.Object);

            // Act
            serviceCollection.AddOpenAIChatClient(
                modelId: "test-model",
                endpoint: new Uri("https://api.openai.com"),
                apiKey: "test-api-key");

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
