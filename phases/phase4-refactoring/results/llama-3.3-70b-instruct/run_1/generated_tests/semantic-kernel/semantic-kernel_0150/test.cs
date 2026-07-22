using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_GetService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Act
            var serviceProvider = services.BuildServiceProvider();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(serviceProvider.GetService<ILoggerFactory>());

            services.AddOllamaChatCompletion("modelId", new Uri("https://example.com"), "serviceId");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
