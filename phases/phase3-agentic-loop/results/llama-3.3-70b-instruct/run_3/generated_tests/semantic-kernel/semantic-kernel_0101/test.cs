using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace Connectors.Google.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_ServiceProvider_GetService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var builderMock = new Mock<IKernelBuilder>();
            builderMock.Setup(b => b.Services).Returns(new ServiceCollection());

            var modelId = "modelId";
            var bearerTokenProvider = () => Task.FromResult("token");
            var location = "location";
            var projectId = "projectId";

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(builderMock.Object, modelId, bearerTokenProvider, location, projectId);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }

        [Fact]
        public void AddVertexAIGeminiChatCompletion_ServiceProvider_GetService_Called_WithBearerKey()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var builderMock = new Mock<IKernelBuilder>();
            builderMock.Setup(b => b.Services).Returns(new ServiceCollection());

            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(builderMock.Object, modelId, bearerKey, location, projectId);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
