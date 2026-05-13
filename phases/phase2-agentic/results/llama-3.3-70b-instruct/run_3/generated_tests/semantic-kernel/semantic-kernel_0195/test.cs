using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.OpenAI.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddOpenAITextEmbeddingGeneration_ServiceProviderGetService_Called()
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
        public async Task AddOpenAITextEmbeddingGeneration_OpenAIClientNotNull_ServiceProviderGetRequiredService_NotCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var openAIClientMock = new Mock<OpenAIClient>();
            serviceProviderMock.Setup(p => p.GetService<OpenAIClient>()).Returns(openAIClientMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", openAIClientMock.Object, "serviceId", 128);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<OpenAIClient>(), Times.Never);
        }

        [Fact]
        public async Task AddOpenAITextEmbeddingGeneration_OpenAIClientNull_ServiceProviderGetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var openAIClientMock = new Mock<OpenAIClient>();
            serviceProviderMock.Setup(p => p.GetService<OpenAIClient>()).Returns(openAIClientMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", null, "serviceId", 128);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<OpenAIClient>(), Times.Once);
        }
    }
}
