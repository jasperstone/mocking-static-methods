using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Http;
using OpenAI;
using Microsoft.Extensions.Logging;

namespace TestOpenAIServiceCollectionExtensions
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddOpenAITextEmbeddingGeneration_ServiceProviderGetService_ReturnsLoggerFactory()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            var result = Microsoft.SemanticKernel.OpenAIServiceCollectionExtensions.AddOpenAITextEmbeddingGeneration(services, "modelId", null, null, null);

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
        }

        [Fact]
        public async Task AddOpenAITextEmbeddingGeneration_ServiceProviderGetService_ThrowsException_WhenLoggerFactoryIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(null);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => Microsoft.SemanticKernel.OpenAIServiceCollectionExtensions.AddOpenAITextEmbeddingGeneration(services, "modelId", null, null, null));
        }
    }
}
