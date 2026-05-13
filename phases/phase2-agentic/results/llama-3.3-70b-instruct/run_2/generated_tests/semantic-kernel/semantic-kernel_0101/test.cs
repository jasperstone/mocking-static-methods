using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_ServiceProviderGetService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            var builder = new Mock<IKernelBuilder>();
            builder.Setup(b => b.Services).Returns(new ServiceCollection());

            var modelId = "model-id";
            var bearerTokenProvider = () => new ValueTask<string>("bearer-token");
            var location = "location";
            var projectId = "project-id";
            var apiVersion = VertexAIVersion.V1;

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builder.Object,
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }

        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_ServiceProviderGetService_NotCalled_WhenLoggerFactoryIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns((ILoggerFactory)null);

            var builder = new Mock<IKernelBuilder>();
            builder.Setup(b => b.Services).Returns(new ServiceCollection());

            var modelId = "model-id";
            var bearerTokenProvider = () => new ValueTask<string>("bearer-token");
            var location = "location";
            var projectId = "project-id";
            var apiVersion = VertexAIVersion.V1;

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builder.Object,
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
