using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.Extensions.Logging;

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
            serviceProviderMock.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var builder = new Mock<IKernelBuilder>();
            var services = new ServiceCollection();
            builder.Setup(b => b.Services).Returns(services);

            var modelId = "model-id";
            var bearerTokenProvider = () => new ValueTask<string>("bearer-token");
            var location = "location";
            var projectId = "project-id";
            var apiVersion = VertexAIVersion.V1;

            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builder.Object,
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion);

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
        }

        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_ServiceProviderGetService_WithHttpClient_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var builder = new Mock<IKernelBuilder>();
            var services = new ServiceCollection();
            builder.Setup(b => b.Services).Returns(services);

            var modelId = "model-id";
            var bearerTokenProvider = () => new ValueTask<string>("bearer-token");
            var location = "location";
            var projectId = "project-id";
            var apiVersion = VertexAIVersion.V1;
            var httpClient = new HttpClient();

            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builder.Object,
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion,
                httpClient: httpClient);

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
