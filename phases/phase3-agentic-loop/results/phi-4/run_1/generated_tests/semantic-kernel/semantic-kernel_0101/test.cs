using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_ShouldAddServiceWithLoggerFactory()
        {
            // Arrange
            var builderMock = new Mock<IKernelBuilder>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            builderMock
                .Setup(b => b.Services)
                .Returns(new ServiceCollection());

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builderMock.Object,
                "modelId",
                async () => await Task.FromResult("bearerToken"),
                "location",
                "projectId",
                apiVersion: VertexAIVersion.V1,
                serviceProvider: serviceProviderMock.Object);

            // Assert
            builderMock.Verify(b => b.Services.AddKeyedSingleton<IChatCompletionService>(
                It.IsAny<string>(),
                It.IsAny<Func<IServiceProvider, object?, IChatCompletionService>>()), Times.Once);

            // Verify that the loggerFactory was requested from the service provider
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
