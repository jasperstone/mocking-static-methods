using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_AddsService()
        {
            // Arrange
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            serviceCollection.AddSingleton<IServiceProvider>(serviceProviderMock.Object);
            kernelBuilderMock.Setup(kb => kb.Services).Returns(serviceCollection);

            var modelId = "test-model";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
            var location = "test-location";
            var projectId = "test-project-id";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                kernelBuilderMock.Object,
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            var serviceProvider = kernelBuilderMock.Object.Services.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

            Assert.NotNull(chatCompletionService);
            Assert.IsType<VertexAIGeminiChatCompletionService>(chatCompletionService);
        }

        [Fact]
        public void AddVertexAIGeminiChatCompletion_WithBearerKey_AddsService()
        {
            // Arrange
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            serviceCollection.AddSingleton<IServiceProvider>(serviceProviderMock.Object);
            kernelBuilderMock.Setup(kb => kb.Services).Returns(serviceCollection);

            var modelId = "test-model";
            var bearerKey = "test-key";
            var location = "test-location";
            var projectId = "test-project-id";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                kernelBuilderMock.Object,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            var serviceProvider = kernelBuilderMock.Object.Services.BuildServiceProvider();
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

            Assert.NotNull(chatCompletionService);
            Assert.IsType<VertexAIGeminiChatCompletionService>(chatCompletionService);
        }
    }
}
