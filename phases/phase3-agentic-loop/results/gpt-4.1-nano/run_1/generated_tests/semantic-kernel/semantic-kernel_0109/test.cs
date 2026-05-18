using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel;

namespace Connectors.Google.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldRegisterServiceAndCallGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a mock ILoggerFactory
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            // Add a mock ILogger
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            // Register a mock IServiceProvider to test GetService
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // Register the mock service provider
            services.AddSingleton(serviceProviderMock.Object);

            // Register a dummy IEmbeddingGenerator to verify registration
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, DummyEmbeddingGenerator>();

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId: "model123",
                bearerTokenProvider: () => new ValueTask<string>("token"),
                location: "us-central1",
                projectId: "project123",
                serviceId: "testService");

            // Create a scope to resolve services
            var scope = result.BuildServiceProvider().CreateScope();

            // Access the service to trigger the GetService call
            var generator = scope.ServiceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            Assert.NotNull(generator);
            // Verify that GetService<ILoggerFactory> was called
            mockLoggerFactory.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.AtLeastOnce);
        }

        // Dummy implementation for testing
        private class DummyEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
        {
            public Task<Embedding<float>> GenerateAsync(string input)
            {
                return Task.FromResult(new Embedding<float>(Array.Empty<float>()));
            }
        }
    }
}
