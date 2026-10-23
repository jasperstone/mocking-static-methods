using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;

namespace Connectors.Google.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_Should_Call_GetService_For_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                               .Returns(loggerFactoryMock.Object);

            // Inject the mock IServiceProvider into the service collection
            services.AddSingleton(serviceProviderMock.Object);

            string modelId = "test-model";
            Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");
            string location = "us-central1";
            string projectId = "test-project";

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId,
                tokenProvider,
                location,
                projectId);

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Retrieve the registered service
            var generator = provider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            Assert.NotNull(generator);
            // Verify that GetService<ILoggerFactory>() was called
            loggerFactoryMock.Verify(lf => lf.GetType(), Times.AtLeastOnce());
        }
    }
}
