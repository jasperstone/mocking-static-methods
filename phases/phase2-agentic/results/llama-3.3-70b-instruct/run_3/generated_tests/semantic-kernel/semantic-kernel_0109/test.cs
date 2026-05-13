using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithValidParameters_RegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "model-id";
            var bearerTokenProvider = () => ValueTask.FromResult("bearer-token");
            var location = "location";
            var projectId = "project-id";
            var apiVersion = VertexAIVersion.V1;

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithValidParametersAndServiceId_RegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "model-id";
            var bearerTokenProvider = () => ValueTask.FromResult("bearer-token");
            var location = "location";
            var projectId = "project-id";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "service-id";

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithValidParametersAndHttpClient_RegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "model-id";
            var bearerTokenProvider = () => ValueTask.FromResult("bearer-token");
            var location = "location";
            var projectId = "project-id";
            var apiVersion = VertexAIVersion.V1;
            var httpClient = new HttpClient();

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion, httpClient: httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_GetService_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "model-id";
            var bearerTokenProvider = () => ValueTask.FromResult("bearer-token");
            var location = "location";
            var projectId = "project-id";
            var apiVersion = VertexAIVersion.V1;
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetService<ILoggerFactory>()).Returns(new LoggerFactory());

            // Act
            services.AddVertexAIEmbeddingGenerator(modelId, bearerTokenProvider, location, projectId, apiVersion);

            // Assert
            serviceProviderMock.Verify(p => p.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
