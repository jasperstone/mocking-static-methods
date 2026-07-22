using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_GetService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            // Act
            services.AddVertexAIEmbeddingGenerator(
                "modelId",
                () => ValueTask.FromResult("bearerToken"),
                "location",
                "projectId",
                VertexAIVersion.V1,
                null,
                null);

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
