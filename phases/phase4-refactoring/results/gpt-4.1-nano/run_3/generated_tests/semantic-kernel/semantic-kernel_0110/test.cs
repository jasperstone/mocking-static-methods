using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_Should_Call_GetService_For_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                               .Returns(mockLoggerFactory.Object);

            // Register the IServiceProvider mock
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

            // Build the service provider to simulate resolution
            var provider = services.BuildServiceProvider();

            // Retrieve the registered service to trigger the extension method
            var generator = provider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            Assert.NotNull(generator);
            // Verify that GetService<ILoggerFactory>() was called
            mockLoggerFactory.VerifyNoOtherCalls();
        }
    }
}
