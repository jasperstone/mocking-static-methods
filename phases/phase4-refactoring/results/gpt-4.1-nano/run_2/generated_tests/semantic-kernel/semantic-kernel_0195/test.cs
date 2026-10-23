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
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Act
            var serviceProvider = serviceProviderMock.Object;
            var registration = services.AddOpenAITextEmbeddingGeneration(
                modelId: "text-embedding-ada-002",
                apiKey: "test-api-key",
                orgId: null,
                serviceId: "test-service",
                dimensions: 128);

            // Build the service provider to trigger the registration lambda
            var provider = services.BuildServiceProvider();

            // Resolve the registered service to invoke the lambda
            var service = provider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
            // Verify that GetService<ILoggerFactory>() was called during resolution
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
