using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_ServiceProvider_GetService_ReturnsLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ServiceProvider_GetService_ThrowsException_WhenLoggerFactoryNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();

            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<ILoggerFactory>());
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_ModelId_BearerKey_Location_ProjectId_ApiVersion_ServiceId_HttpClient_ReturnsServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            var result = VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(services, modelId, bearerKey, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            Assert.Same(services, result);
        }
    }
}
