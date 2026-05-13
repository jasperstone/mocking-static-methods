using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_ServiceProvider_GetService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = new Mock<ILoggerFactory>();
            serviceProvider.GetService<ILoggerFactory>()?.Returns(loggerFactory.Object);

            // Act
            services.AddHuggingFaceImageToText("model", null, null, null, null);

            // Assert
            loggerFactory.Verify(x => x.CreateLogger(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void AddHuggingFaceImageToText_ServiceProvider_GetService_Called_WithEndpoint()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = new Mock<ILoggerFactory>();
            serviceProvider.GetService<ILoggerFactory>()?.Returns(loggerFactory.Object);

            // Act
            services.AddHuggingFaceImageToText(new Uri("https://example.com"), null, null, null);

            // Assert
            loggerFactory.Verify(x => x.CreateLogger(It.IsAny<string>()), Times.Once);
        }
    }
}
