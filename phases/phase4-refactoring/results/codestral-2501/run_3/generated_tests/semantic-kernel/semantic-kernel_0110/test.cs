using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldAddService()
        {
            // Arrange
            var serviceCollectionMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            Func<IServiceProvider, object, IEmbeddingGenerator<string, Embedding<float>>> factory = null;

            serviceCollectionMock.Setup(sc => sc.Add(It.IsAny<ServiceDescriptor>()))
                .Callback<ServiceDescriptor>(sd => factory = sd.ImplementationFactory as Func<IServiceProvider, object, IEmbeddingGenerator<string, Embedding<float>>>);

            // Act
            VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                serviceCollectionMock.Object,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            serviceCollectionMock.Verify(
                sc => sc.Add(It.IsAny<ServiceDescriptor>()),
                Times.Once);

            Assert.NotNull(factory);
            var embeddingGenerator = factory(serviceProviderMock.Object, null);
            Assert.NotNull(embeddingGenerator);
        }
    }
}
