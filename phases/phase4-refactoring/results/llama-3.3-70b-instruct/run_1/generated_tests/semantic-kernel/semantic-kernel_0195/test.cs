using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_ServiceProvider_GetService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", "apiKey", "orgId", "serviceId", 128);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }

        [Fact]
        public void AddOpenAITextEmbeddingGeneration_OpenAIClient_Null_ServiceProvider_GetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var openAIClientMock = new Mock<OpenAIClient>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(OpenAIClient))).Returns(openAIClientMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", openAIClientMock.Object, "serviceId", 128);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(OpenAIClient)), Times.Once);
        }
    }
}
