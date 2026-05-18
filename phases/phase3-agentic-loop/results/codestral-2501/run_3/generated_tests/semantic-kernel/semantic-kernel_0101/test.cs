using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var kernelBuilder = new Mock<IKernelBuilder>();
            kernelBuilder.Setup(kb => kb.Services).Returns(serviceCollection);

            var modelId = "test-model";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                kernelBuilder.Object,
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            var service = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddVertexAIGeminiChatCompletion_WithBearerKey_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var kernelBuilder = new Mock<IKernelBuilder>();
            kernelBuilder.Setup(kb => kb.Services).Returns(serviceCollection);

            var modelId = "test-model";
            var bearerKey = "test-key";
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                kernelBuilder.Object,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            var service = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddVertexAIGeminiChatCompletion_ShouldCallGetService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceCollection.AddSingleton(loggerFactoryMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var kernelBuilder = new Mock<IKernelBuilder>();
            kernelBuilder.Setup(kb => kb.Services).Returns(serviceCollection);

            var modelId = "test-model";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                kernelBuilder.Object,
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            loggerFactoryMock.Verify(lf => lf.CreateLogger(It.IsAny<string>()), Times.Once);
        }
    }
}
