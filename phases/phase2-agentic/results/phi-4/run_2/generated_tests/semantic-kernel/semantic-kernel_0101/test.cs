using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            var builderMock = new Mock<IKernelBuilder>();
            builderMock.Setup(b => b.Services).Returns(serviceProviderMock.Object);

            string modelId = "test-model-id";
            Func<ValueTask<string>> bearerTokenProvider = () => new ValueTask<string>("test-token");
            string location = "test-location";
            string projectId = "test-project-id";

            // Act
            builderMock.Object.AddVertexAIGeminiChatCompletion(
                modelId,
                bearerTokenProvider,
                location,
                projectId);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
