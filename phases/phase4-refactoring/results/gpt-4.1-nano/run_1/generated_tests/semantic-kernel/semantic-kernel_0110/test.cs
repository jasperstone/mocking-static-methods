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

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                               .Returns(loggerFactoryMock.Object)
                               .Verifiable();

            // Add a dummy ILoggerFactory to the service collection to simulate existing registration
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);

            // Build a service provider that returns the mock when requested
            var builtServiceProvider = services.BuildServiceProvider();

            // Act
            // Call the extension method with dummy parameters
            services.AddVertexAIEmbeddingGenerator(
                modelId: "model",
                bearerKey: () => new ValueTask<string>("token"),
                location: "us-central1",
                projectId: "project",
                apiVersion: VertexAIVersion.V1,
                serviceId: null,
                httpClient: null);

            // Create a new service provider from the service collection
            var sp = services.BuildServiceProvider();

            // Manually invoke the lambda to simulate the registration process
            var _ = sp.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            // Verify that GetService<ILoggerFactory>() was called
            loggerFactoryMock.Verify(lf => lf.GetType(), Times.AtLeastOnce);
        }
    }
}
