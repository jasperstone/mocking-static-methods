using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ServiceProviderGetService_CalledOnce()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<Microsoft.Extensions.Logging.ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            var serviceProvider = serviceProviderMock.Object;

            // Act
            var provider = services.BuildServiceProvider();
            services.AddHuggingFaceTextEmbeddingGeneration(
                new Uri("https://example.com"),
                "apiKey",
                "serviceId",
                new HttpClient());

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)), Times.Once);
        }
    }
}
