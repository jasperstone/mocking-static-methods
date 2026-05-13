using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.OpenAI.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_ServiceProviderGetService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", "apiKey", "orgId", "serviceId", 128);

            // Assert
            serviceProviderMock.Verify(p => p.GetService<ILoggerFactory>(), Times.Once);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_OpenAIClientNotNull_ServiceProviderGetRequiredService_NotCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var openAIClientMock = new Mock<OpenAIClient>();
            serviceProviderMock.Setup(p => p.GetService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", openAIClientMock.Object, "serviceId", 128);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<OpenAIClient>(), Times.Never);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_OpenAIClientNull_ServiceProviderGetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var openAIClientMock = new Mock<OpenAIClient>();
            serviceProviderMock.Setup(p => p.GetService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);
            serviceProviderMock.Setup(p => p.GetRequiredService<OpenAIClient>()).Returns(openAIClientMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", null, "serviceId", 128);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<OpenAIClient>(), Times.Once);
        }
    }
}
