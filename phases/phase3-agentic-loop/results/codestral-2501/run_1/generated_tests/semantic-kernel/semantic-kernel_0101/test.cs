using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.ChatCompletion;

public class VertexAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_ShouldRegisterService()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var kernelBuilder = new Mock<IKernelBuilder>();
        kernelBuilder.Setup(b => b.Services).Returns(serviceCollection);

        string modelId = "test-model";
        Func<ValueTask<string>> bearerTokenProvider = async () => "test-token";
        string location = "test-location";
        string projectId = "test-project";
        VertexAIVersion apiVersion = VertexAIVersion.V1;
        string serviceId = "test-service";
        HttpClient httpClient = new HttpClient();

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
        var serviceDescriptor = serviceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IChatCompletionService));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
        Assert.Equal(serviceId, serviceDescriptor.ServiceKey);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerKey_ShouldRegisterService()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var kernelBuilder = new Mock<IKernelBuilder>();
        kernelBuilder.Setup(b => b.Services).Returns(serviceCollection);

        string modelId = "test-model";
        string bearerKey = "test-key";
        string location = "test-location";
        string projectId = "test-project";
        VertexAIVersion apiVersion = VertexAIVersion.V1;
        string serviceId = "test-service";
        HttpClient httpClient = new HttpClient();

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
        var serviceDescriptor = serviceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IChatCompletionService));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
        Assert.Equal(serviceId, serviceDescriptor.ServiceKey);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_ShouldCallGetServiceOnServiceProvider()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        serviceCollection.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var kernelBuilder = new Mock<IKernelBuilder>();
        kernelBuilder.Setup(b => b.Services).Returns(serviceCollection);

        string modelId = "test-model";
        Func<ValueTask<string>> bearerTokenProvider = async () => "test-token";
        string location = "test-location";
        string projectId = "test-project";
        VertexAIVersion apiVersion = VertexAIVersion.V1;
        string serviceId = "test-service";
        HttpClient httpClient = new HttpClient();

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
        loggerFactoryMock.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
    }
}
