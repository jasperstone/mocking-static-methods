using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Moq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_ServiceProviderGetService_CalledOnce()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var services = new ServiceCollection();
            var builder = services.BuildServiceProvider();
            var modelId = "model-id";
            var bearerTokenProvider = () => new ValueTask<string>("bearer-token");
            var location = "location";
            var projectId = "project-id";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddVertexAIGeminiChatCompletion(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
        }

        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_WithBearerKey_ServiceProviderGetService_CalledOnce()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var services = new ServiceCollection();
            var builder = services.BuildServiceProvider();
            var modelId = "model-id";
            var bearerKey = "bearer-key";
            var location = "location";
            var projectId = "project-id";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "service-id";
            var httpClient = new HttpClient();

            // Act
            services.AddVertexAIGeminiChatCompletion(modelId, bearerKey, location, projectId, apiVersion, serviceId, httpClient);

            // Assert
            serviceProviderMock.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
