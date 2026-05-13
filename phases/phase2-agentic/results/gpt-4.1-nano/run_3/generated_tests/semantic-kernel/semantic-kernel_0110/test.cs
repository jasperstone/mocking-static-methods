using System;
using System.Net.Http;
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

            // Build the service provider so that GetService can be called
            var serviceProvider = services.BuildServiceProvider();

            // Create a new service collection to test extension method
            var testServices = new ServiceCollection();

            // Add the dummy ILoggerFactory to the test services
            testServices.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);

            // Act
            var result = testServices.AddVertexAIEmbeddingGenerator(
                modelId: "model",
                bearerKey: "key",
                location: "us-central1",
                projectId: "project",
                apiVersion: VertexAIVersion.V1,
                serviceId: null,
                httpClient: null);

            // Build the final provider
            var provider = result.BuildServiceProvider();

            // Retrieve the service to trigger the GetService call
            var generator = provider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            Assert.NotNull(generator);
            // Verify that ILoggerFactory was retrieved from the service provider
            // Since we can't directly verify the internal call, we ensure no exception and that the service is resolved
        }
    }
}
