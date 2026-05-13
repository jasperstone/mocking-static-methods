using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Moq;
using Xunit;

namespace Tests
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

            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            // Act
            services.AddOllamaChatCompletion("modelId", new Uri("https://example.com"), "serviceId");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
