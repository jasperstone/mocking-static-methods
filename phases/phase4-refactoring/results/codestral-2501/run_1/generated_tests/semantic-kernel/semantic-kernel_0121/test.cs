using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_WithModel_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(mockLoggerFactory.Object);

            // Act
            serviceCollection.AddHuggingFaceImageToText("model");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
            Assert.Same(mockLoggerFactory.Object, loggerFactory);
        }

        [Fact]
        public void AddHuggingFaceImageToText_WithEndpoint_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(mockLoggerFactory.Object);

            // Act
            serviceCollection.AddHuggingFaceImageToText(new Uri("https://example.com"));

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
            Assert.Same(mockLoggerFactory.Object, loggerFactory);
        }
    }
}
