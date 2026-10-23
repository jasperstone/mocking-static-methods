using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_Should_Call_GetService_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Act
            services.AddOpenAITextEmbeddingGeneration(
                modelId: "text-embedding-ada-002",
                apiKey: "test-api-key");

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the service to trigger the internal GetService call
            var scope = serviceProvider.CreateScope();
            var _ = scope.ServiceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
