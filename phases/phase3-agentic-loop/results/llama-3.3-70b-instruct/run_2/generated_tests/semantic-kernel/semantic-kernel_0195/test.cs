using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
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
        public async Task AddOpenAITextEmbeddingGeneration_ServiceProviderGetService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", "apiKey", "orgId", "serviceId", 128);

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
        }

        [Fact]
        public async Task AddOpenAITextEmbeddingGeneration_OpenAIClientProvided_ServiceProviderGetRequiredService_NotCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var openAIClientMock = new Mock<OpenAIClient>();
            serviceProviderMock.Setup(p => p.GetService(typeof(OpenAIClient))).Returns(openAIClientMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", openAIClientMock.Object, "serviceId", 128);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService(typeof(OpenAIClient)), Times.Never);
        }

        [Fact]
        public async Task AddOpenAITextEmbeddingGeneration_OpenAIClientNotProvided_ServiceProviderGetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var openAIClientMock = new Mock<OpenAIClient>();
            serviceProviderMock.Setup(p => p.GetService(typeof(OpenAIClient))).Returns(openAIClientMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration("modelId", null, "serviceId", 128);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService(typeof(OpenAIClient)), Times.Once);
        }
    }
}
