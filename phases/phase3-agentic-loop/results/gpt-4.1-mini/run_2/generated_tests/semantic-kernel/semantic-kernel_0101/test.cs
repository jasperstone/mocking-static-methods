using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Tests.Connectors.Google.Extensions;

public class VertexAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_RegistersServiceAndCallsGetService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add a mock ILoggerFactory to the service collection to be resolved by serviceProvider.GetService<ILoggerFactory>()
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        services.AddSingleton(loggerFactoryMock.Object);

        var builderMock = new Mock<IKernelBuilder>();
        builderMock.SetupGet(b => b.Services).Returns(services);

        var modelId = "test-model";
        Func<ValueTask<string>> bearerTokenProvider = () => new ValueTask<string>("token");
        var location = "us-central1";
        var projectId = "test-project";
        var apiVersion = VertexAIVersion.V1;
        string? serviceId = null; // Use null to register default service

        var builder = builderMock.Object;

        // Act
        var returnedBuilder = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder,
            modelId,
            bearerTokenProvider,
            location,
            projectId,
            apiVersion,
            serviceId,
            null);

        // Build the service provider to resolve the service and trigger the factory delegate
        var serviceProvider = services.BuildServiceProvider();

        // Resolve the IChatCompletionService without a key (default)
        var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

        // Assert
        Assert.Same(builder, returnedBuilder);
        Assert.NotNull(chatCompletionService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatCompletionService);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerKey_RegistersServiceAndCallsGetService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add a mock ILoggerFactory to the service collection to be resolved by serviceProvider.GetService<ILoggerFactory>()
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        services.AddSingleton(loggerFactoryMock.Object);

        var builderMock = new Mock<IKernelBuilder>();
        builderMock.SetupGet(b => b.Services).Returns(services);

        var modelId = "test-model";
        var bearerKey = "test-bearer-key";
        var location = "us-central1";
        var projectId = "test-project";
        var apiVersion = VertexAIVersion.V1;
        string? serviceId = null; // Use null to register default service

        var builder = builderMock.Object;

        // Act
        var returnedBuilder = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder,
            modelId,
            bearerKey,
            location,
            projectId,
            apiVersion,
            serviceId,
            null);

        // Build the service provider to resolve the service and trigger the factory delegate
        var serviceProvider = services.BuildServiceProvider();

        // Resolve the IChatCompletionService without a key (default)
        var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

        // Assert
        Assert.Same(builder, returnedBuilder);
        Assert.NotNull(chatCompletionService);
        Assert.IsType<VertexAIGeminiChatCompletionService>(chatCompletionService);
    }
}
