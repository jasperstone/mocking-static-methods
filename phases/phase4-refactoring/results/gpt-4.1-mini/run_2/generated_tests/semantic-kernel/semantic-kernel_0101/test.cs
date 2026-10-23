using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.Google.Extensions;

public class VertexAIKernelBuilderExtensionsTests
{
    private class TestKernelBuilder : IKernelBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
        public IKernelBuilderPlugins Plugins { get; } = new TestKernelBuilderPlugins();
    }

    private class TestKernelBuilderPlugins : IKernelBuilderPlugins
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_RegistersServiceAndCallsGetService()
    {
        // Arrange
        var builder = new TestKernelBuilder();
        var modelId = "test-model";
        Func<ValueTask<string>> bearerTokenProvider = () => new ValueTask<string>("token");
        var location = "us-central1";
        var projectId = "test-project";
        var apiVersion = VertexAIVersion.V1;
        var serviceId = "test-service";

        // Add a mock logger factory to the service collection to verify GetService call returns non-null
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        builder.Services.AddSingleton(mockLoggerFactory.Object);

        // Act
        var returnedBuilder = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder,
            modelId,
            bearerTokenProvider,
            location,
            projectId,
            apiVersion,
            serviceId,
            httpClient: null);

        // Assert
        Assert.Same(builder, returnedBuilder);

        // Check that the service collection contains a registration for IChatCompletionService
        bool serviceRegistered = false;
        foreach (var serviceDescriptor in builder.Services)
        {
            if (serviceDescriptor.ServiceType == typeof(IChatCompletionService))
            {
                serviceRegistered = true;
                break;
            }
        }
        Assert.True(serviceRegistered, "IChatCompletionService should be registered in the service collection.");
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerKey_RegistersServiceAndCallsGetService()
    {
        // Arrange
        var builder = new TestKernelBuilder();
        var modelId = "test-model";
        var bearerKey = "test-bearer-key";
        var location = "us-central1";
        var projectId = "test-project";
        var apiVersion = VertexAIVersion.V1;
        var serviceId = "test-service";

        // Add a mock logger factory to the service collection to verify GetService call returns non-null
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        builder.Services.AddSingleton(mockLoggerFactory.Object);

        // Act
        var returnedBuilder = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
            builder,
            modelId,
            bearerKey,
            location,
            projectId,
            apiVersion,
            serviceId,
            httpClient: null);

        // Assert
        Assert.Same(builder, returnedBuilder);

        // Check that the service collection contains a registration for IChatCompletionService
        bool serviceRegistered = false;
        foreach (var serviceDescriptor in builder.Services)
        {
            if (serviceDescriptor.ServiceType == typeof(IChatCompletionService))
            {
                serviceRegistered = true;
                break;
            }
        }
        Assert.True(serviceRegistered, "IChatCompletionService should be registered in the service collection.");
    }
}
