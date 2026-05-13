using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Connectors.Ollama.Client;
using Connectors.Ollama.Extensions;

namespace Connectors.Ollama.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = new ServiceProvider(services.BuildServiceProvider());
            var ollamaClient = new OllamaApiClient(new HttpClient(), new LoggerFactory().CreateLogger("Test"));
            var serviceId = "TestServiceId";

            // Act
            services.AddOllamaTextEmbeddingGeneration(ollamaClient, serviceId);

            // Assert
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(OllamaApiClient))).Returns(ollamaClient);
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetServiceCalled_WithNullOllamaClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = new ServiceProvider(services.BuildServiceProvider());
            var serviceId = "TestServiceId";

            // Act
            services.AddOllamaTextEmbeddingGeneration(null, serviceId);

            // Assert
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(OllamaApiClient))).Returns((OllamaApiClient)null);
            Assert.Throws<InvalidOperationException>(() => services.AddOllamaTextEmbeddingGeneration(null, serviceId));
        }
    }
}
