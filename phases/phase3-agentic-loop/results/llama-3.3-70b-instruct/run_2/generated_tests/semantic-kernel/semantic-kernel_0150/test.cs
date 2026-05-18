using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_ServiceProvider_GetService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock.Setup(s => s.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);

            // Act
            services.AddOllamaChatCompletion("modelId", new Uri("https://example.com"), "serviceId");
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProviderMock.Verify(s => s.GetService(typeof(ILoggerFactory)), Times.Once);
        }

        [Fact]
        public void AddOllamaChatCompletion_ServiceProvider_GetService_ReturnsNull_LoggerFactoryIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock.Setup(s => s.GetService(typeof(ILoggerFactory))).Returns((ILoggerFactory)null);

            services.AddSingleton<ILoggerFactory>(null);

            // Act
            services.AddOllamaChatCompletion("modelId", new Uri("https://example.com"), "serviceId");
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProviderMock.Verify(s => s.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
