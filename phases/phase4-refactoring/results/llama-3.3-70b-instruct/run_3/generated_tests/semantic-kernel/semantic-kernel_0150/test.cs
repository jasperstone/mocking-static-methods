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
                .Setup(p => p.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            var serviceProvider = serviceProviderMock.Object;

            // Act
            services.AddOllamaChatCompletion("modelId", new Uri("https://example.com"), "serviceId");
            var provider = services.BuildServiceProvider();

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
