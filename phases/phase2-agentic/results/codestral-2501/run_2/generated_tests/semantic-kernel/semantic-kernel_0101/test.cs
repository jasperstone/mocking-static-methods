using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_ShouldAddService()
        {
            // Arrange
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var serviceCollectionMock = new Mock<IServiceCollection>();
            kernelBuilderMock.Setup(kb => kb.Services).Returns(serviceCollectionMock.Object);
            var modelId = "modelId";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("token"));
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
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
            serviceCollectionMock.Verify(sc => sc.AddKeyedSingleton<IChatCompletionService>(
                serviceId,
                It.IsAny<Func<IServiceProvider, string, IChatCompletionService>>()), Times.Once);
        }

        [Fact]
        public void AddVertexAIGeminiChatCompletion_WithBearerKey_ShouldAddService()
        {
            // Arrange
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var serviceCollectionMock = new Mock<IServiceCollection>();
            kernelBuilderMock.Setup(kb => kb.Services).Returns(serviceCollectionMock.Object);
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
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
            serviceCollectionMock.Verify(sc => sc.AddKeyedSingleton<IChatCompletionService>(
                serviceId,
                It.IsAny<Func<IServiceProvider, string, IChatCompletionService>>()), Times.Once);
        }

        [Fact]
        public void AddVertexAIEmbeddingGeneration_WithBearerTokenProvider_ShouldAddService()
        {
            // Arrange
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var serviceCollectionMock = new Mock<IServiceCollection>();
            kernelBuilderMock.Setup(kb => kb.Services).Returns(serviceCollectionMock.Object);
            var modelId = "modelId";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("token"));
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIEmbeddingGeneration(
                kernelBuilderMock.Object,
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            serviceCollectionMock.Verify(sc => sc.AddKeyedSingleton<ITextEmbeddingGenerationService>(
                serviceId,
                It.IsAny<Func<IServiceProvider, string, ITextEmbeddingGenerationService>>()), Times.Once);
        }

        [Fact]
        public void AddVertexAIEmbeddingGeneration_WithBearerKey_ShouldAddService()
        {
            // Arrange
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var serviceCollectionMock = new Mock<IServiceCollection>();
            kernelBuilderMock.Setup(kb => kb.Services).Returns(serviceCollectionMock.Object);
            var modelId = "modelId";
            var bearerKey = "bearerKey";
            var location = "location";
            var projectId = "projectId";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIEmbeddingGeneration(
                kernelBuilderMock.Object,
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient);

            // Assert
            serviceCollectionMock.Verify(sc => sc.AddKeyedSingleton<ITextEmbeddingGenerationService>(
                serviceId,
                It.IsAny<Func<IServiceProvider, string, ITextEmbeddingGenerationService>>()), Times.Once);
        }
    }
}
