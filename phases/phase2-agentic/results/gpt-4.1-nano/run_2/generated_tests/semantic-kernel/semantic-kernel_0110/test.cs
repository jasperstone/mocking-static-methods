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

            // Add a mock ILoggerFactory to the service collection
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(mockLoggerFactory.Object);

            // Build the service provider so that GetService can be called
            var serviceProvider = services.BuildServiceProvider();

            // Create a new service collection to test extension method
            var testServices = new ServiceCollection();

            // Add a dummy implementation for AddKeyedSingleton to capture the serviceProvider
            // Since AddKeyedSingleton is an extension method, we assume it adds services accordingly.
            // For testing, we focus on whether GetService<ILoggerFactory>() is called during registration.

            // Act
            testServices.AddVertexAIEmbeddingGenerator(
                modelId: "model",
                bearerKey: "key",
                location: "us-central1",
                projectId: "project",
                apiVersion: VertexAIVersion.V1,
                serviceId: null,
                httpClient: new HttpClient());

            var provider = testServices.BuildServiceProvider();

            // Assert
            // Verify that GetService<ILoggerFactory>() returns the mock ILoggerFactory
            var loggerFactory = provider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
            Assert.Equal(mockLoggerFactory.Object, loggerFactory);
        }
    }
}
