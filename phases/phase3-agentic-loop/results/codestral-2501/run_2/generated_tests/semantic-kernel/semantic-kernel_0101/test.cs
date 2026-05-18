using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.Google;

public class VertexAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_CallsGetService()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

        var mockServiceCollection = new Mock<IServiceCollection>();
        var mockKernelBuilder = new Mock<IKernelBuilder>();
        mockKernelBuilder.Setup(kb => kb.Services).Returns(mockServiceCollection.Object);

        var modelId = "test-model";
        var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
        var location = "test-location";
        var projectId = "test-project";
        var apiVersion = VertexAIVersion.V1;
        var serviceId = "test-service";
        var httpClient = new HttpClient();

        // Act
        VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            mockKernelBuilder.Object,
            modelId,
            bearerTokenProvider,
            location,
            projectId,
            apiVersion,
            serviceId,
            httpClient);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerKey_CallsGetService()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

        var mockServiceCollection = new Mock<IServiceCollection>();
        var mockKernelBuilder = new Mock<IKernelBuilder>();
        mockKernelBuilder.Setup(kb => kb.Services).Returns(mockServiceCollection.Object);

        var modelId = "test-model";
        var bearerKey = "test-key";
        var location = "test-location";
        var projectId = "test-project";
        var apiVersion = VertexAIVersion.V1;
        var serviceId = "test-service";
        var httpClient = new HttpClient();

        // Act
        VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            mockKernelBuilder.Object,
            modelId,
            bearerKey,
            location,
            projectId,
            apiVersion,
            serviceId,
            httpClient);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
    }
}
