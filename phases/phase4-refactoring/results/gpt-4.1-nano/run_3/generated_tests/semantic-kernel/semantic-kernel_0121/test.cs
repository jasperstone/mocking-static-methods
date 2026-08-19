using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.ImageToText;
using Microsoft.SemanticKernel.Connectors.HuggingFace;

namespace SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_Should_Call_GetService_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            // Register the IServiceProvider to return the mock ILoggerFactory
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            var result = services.AddHuggingFaceImageToText(
                model: "test-model",
                endpoint: new Uri("https://test-endpoint"),
                apiKey: "test-api-key",
                serviceId: "test-service",
                httpClient: null);

            // Build the provider to trigger the lambda
            var provider = services.BuildServiceProvider();

            // Retrieve the service to invoke the lambda
            var service = provider.GetService<IImageToTextService>();

            // Assert
            Assert.NotNull(service);
            // Verify that GetService<ILoggerFactory>() was called
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
