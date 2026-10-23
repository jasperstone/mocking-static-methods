using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_ServiceProviderGetServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var builder = new KernelBuilder(serviceProviderMock.Object);
            var modelId = "model-id";
            var bearerTokenProvider = () => new ValueTask<string>("bearer-token");
            var location = "location";
            var projectId = "project-id";

            // Act
            builder.AddVertexAIGeminiChatCompletion(modelId, bearerTokenProvider, location, projectId);

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
        }

        [Fact]
        public void AddVertexAIGeminiChatCompletion_ServiceProviderGetServiceCalled_WithBearerKey()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var builder = new KernelBuilder(serviceProviderMock.Object);
            var modelId = "model-id";
            var bearerKey = "bearer-key";
            var location = "location";
            var projectId = "project-id";

            // Act
            builder.AddVertexAIGeminiChatCompletion(modelId, bearerKey, location, projectId);

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
