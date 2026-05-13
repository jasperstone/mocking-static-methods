using System;
using System.Net.Http;
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
        public void AddVertexAIEmbeddingGenerator_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            var modelId = "test-model-id";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
            var location = "test-location";
            var projectId = "test-project-id";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
