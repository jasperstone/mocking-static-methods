using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_Should_Call_GetService_For_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a dummy ILoggerFactory to the service collection
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            string modelId = "test-model";
            Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");
            string location = "us-central1";
            string projectId = "test-project";

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId,
                tokenProvider,
                location,
                projectId);

            // Assert
            var serviceProvider = result.BuildServiceProvider();

            // Verify that GetService<ILoggerFactory>() returns the mocked ILoggerFactory
            var loggerFactoryFromProvider = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactoryFromProvider);
            Assert.Equal(loggerFactoryMock.Object, loggerFactoryFromProvider);
        }
    }
}
