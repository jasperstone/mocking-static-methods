using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.TextGeneration;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextGeneration_ShouldRegisterServiceAndCallGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Create a mock ILoggerFactory
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Create a mock IServiceProvider
            var mockServiceProvider = new Mock<IServiceProvider>();
            // Setup GetService<ILoggerFactory>() to return the mock logger factory
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            // Add the mock IServiceProvider to the services
            services.AddSingleton(mockServiceProvider.Object);

            // Act
            services.AddHuggingFaceTextGeneration(
                model: "test-model",
                endpoint: new Uri("https://fakeendpoint.com"),
                apiKey: "test-api-key",
                serviceId: "test-service",
                httpClient: new HttpClient());

            // Build the provider
            var provider = services.BuildServiceProvider();

            // Retrieve the service
            var service = provider.GetService<ITextGenerationService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextGenerationService>(service);
        }
    }
}
