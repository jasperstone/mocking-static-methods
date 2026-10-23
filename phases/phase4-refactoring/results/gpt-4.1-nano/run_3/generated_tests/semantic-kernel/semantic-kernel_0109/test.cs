using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace VertexAIEmbeddingGeneratorTests
{
    public class AddVertexAIEmbeddingGeneratorTests
    {
        [Fact]
        public void AddsServiceAndCallsGetServiceLoggerFactory()
        {
            // Arrange
            var servicesMock = new ServiceCollection();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Setup GetService<ILoggerFactory>() to return the mock
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Use the real ServiceCollection to test registration
            var services = new ServiceCollection();

            // Add a dummy singleton to simulate the service provider
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId: "model123",
                bearerKey: "token",
                location: "us-central1",
                projectId: "project123",
                apiVersion: VertexAIVersion.V1,
                serviceId: null,
                httpClient: null);

            // Build the provider to trigger the registration
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, s => s.ServiceType == typeof(IEmbeddingGenerator<string, Embedding<float>>));
            // Verify that GetService<ILoggerFactory>() was called during registration
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
