using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddOpenAITextEmbeddingGeneration_ServiceProvider_GetService_Called()
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
        public async Task AddOpenAITextEmbeddingGeneration_ServiceProvider_GetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var openAIClientMock = new Mock<OpenAIClient>();
            serviceProviderMock.Setup(p => p.GetRequiredService<OpenAIClient>()).Returns(openAIClientMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", null, "serviceId", 128);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<OpenAIClient>(), Times.Once);
        }

        [Fact]
        public async Task AddOpenAITextToImage_ServiceProvider_GetService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOpenAITextToImage("apiKey", "orgId", "modelId", "serviceId");

            // Assert
            serviceProviderMock.Verify(p => p.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
