using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
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
        public async Task AddHuggingFaceImageToText_ServiceProvider_GetService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);
            var serviceProviderWithLogger = services.BuildServiceProvider();

            // Act
            services.AddHuggingFaceImageToText("model", null, null, null, null);

            // Assert
            loggerFactoryMock.Verify(l => l.CreateLogger(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AddHuggingFaceImageToText_ServiceProvider_GetService_Called_WithEndpoint()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);
            var serviceProviderWithLogger = services.BuildServiceProvider();
            var endpoint = new Uri("https://example.com");

            // Act
            services.AddHuggingFaceImageToText(endpoint, null, null, null);

            // Assert
            loggerFactoryMock.Verify(l => l.CreateLogger(It.IsAny<string>()), Times.Once);
        }
    }
}
