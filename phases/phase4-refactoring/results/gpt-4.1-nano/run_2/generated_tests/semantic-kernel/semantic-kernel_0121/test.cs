using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.TextGeneration;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextGeneration_Should_Call_GetService_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(mockLoggerFactory.Object);

            // Setup the services to return the mock IServiceProvider when requested
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddHuggingFaceTextGeneration(
                model: "test-model",
                endpoint: new Uri("https://test-endpoint"),
                apiKey: "test-api-key",
                serviceId: "test-service",
                httpClient: new HttpClient());

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Retrieve the registered service
            var service = provider.GetService<ITextGenerationService>();

            // Assert
            Assert.NotNull(service);
            // Verify that GetService<ILoggerFactory>() was called
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.AtLeastOnce);
        }
    }
}
