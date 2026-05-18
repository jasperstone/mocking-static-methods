using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var servicesMock = new Mock<IServiceCollection>();
            var modelId = "test-model";
            var location = "us-central1";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            string? serviceId = null;

            Func<ValueTask<string>> bearerTokenProvider = () => new ValueTask<string>("token");

            // Capture the factory delegate passed to AddKeyedSingleton
            Func<IServiceProvider, string?, object>? capturedFactory = null;

            servicesMock
                .Setup(s => s.Add(It.IsAny<ServiceDescriptor>()))
                .Callback<ServiceDescriptor>(sd =>
                {
                    // The factory is stored in sd.ImplementationFactory
                    capturedFactory = (sp, id) =>
                    {
                        // The AddKeyedSingleton likely uses a factory with signature (IServiceProvider, string?) => object
                        // But ServiceDescriptor.ImplementationFactory is Func<IServiceProvider, object>
                        // So we only have IServiceProvider parameter here.
                        // We will just invoke the factory with the IServiceProvider.
                        return sd.ImplementationFactory!(sp);
                    };
                })
                .Returns(servicesMock.Object);

            // Act
            var returnedServices = VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                servicesMock.Object,
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient: null);

            // Assert the returned services is the same instance
            Assert.Same(servicesMock.Object, returnedServices);

            // Now test the factory delegate calls GetService<ILoggerFactory> on IServiceProvider
            Assert.NotNull(capturedFactory);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object)
                .Verifiable();

            // Invoke the factory delegate with the mocked service provider
            var instance = capturedFactory!(serviceProviderMock.Object, serviceId);

            // Verify GetService was called
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);

            Assert.NotNull(instance);
        }

        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var servicesMock = new Mock<IServiceCollection>();
            var modelId = "test-model";
            var location = "us-central1";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            string? serviceId = null;

            string bearerKey = "test-bearer-key";

            Func<IServiceProvider, string?, object>? capturedFactory = null;

            servicesMock
                .Setup(s => s.Add(It.IsAny<ServiceDescriptor>()))
                .Callback<ServiceDescriptor>(sd =>
                {
                    capturedFactory = (sp, id) => sd.ImplementationFactory!(sp);
                })
                .Returns(servicesMock.Object);

            // Act
            var returnedServices = VertexAIServiceCollectionExtensions.AddVertexAIEmbeddingGenerator(
                servicesMock.Object,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient: null);

            // Assert
            Assert.Same(servicesMock.Object, returnedServices);
            Assert.NotNull(capturedFactory);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object)
                .Verifiable();

            var instance = capturedFactory!(serviceProviderMock.Object, serviceId);

            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);

            Assert.NotNull(instance);
        }
    }
}
