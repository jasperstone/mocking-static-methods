using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ServiceProvider_GetService_ReturnsILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // Act
            var loggerFactoryFromServiceProvider = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(loggerFactoryFromServiceProvider);
            Assert.Same(loggerFactory, loggerFactoryFromServiceProvider);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ServiceProvider_GetService_ThrowsException_WhenLoggerFactoryNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<ILoggerFactory>());
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ServiceProvider_GetService_ReturnsNull_WhenLoggerFactoryNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var loggerFactoryFromServiceProvider = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.Null(loggerFactoryFromServiceProvider);
        }
    }
}
