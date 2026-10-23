using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Logging;
using OllamaSharp;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ServiceProvider_GetService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var ollamaClientMock = new Mock<OllamaApiClient>(new Uri("https://example.com"));
            serviceProviderMock.Setup(p => p.GetService(typeof(OllamaApiClient))).Returns(ollamaClientMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOllamaTextEmbeddingGeneration(ollamaClient: null);

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
            serviceProviderMock.Verify(p => p.GetService(typeof(OllamaApiClient)), Times.Once);
        }
    }
}
