using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_Should_Call_GetService_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                               .Returns(loggerFactoryMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId: "model",
                bearerKey: "key",
                location: "us-central1",
                projectId: "project",
                apiVersion: VertexAIVersion.V1,
                serviceId: null,
                httpClient: null);

            // Assert
            // Verify that GetService<ILoggerFactory>() was called
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
            Assert.NotNull(result);
        }
    }
}
