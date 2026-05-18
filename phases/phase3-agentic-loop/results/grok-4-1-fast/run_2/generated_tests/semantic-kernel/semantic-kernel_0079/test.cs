using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI.Extensions;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.AzureOpenAI.Extensions.Tests;

public class AzureOpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureOpenAIChatClient_CallsGetServiceForILoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockServiceProvider = new MockServiceProvider();

        // Register mock service provider so factory uses it
        services.AddSingleton<IServiceProvider>(mockServiceProvider);

        // Act
        var result = services.AddAzureOpenAIChatClient(
            deploymentName: "gpt-35-turbo",
            endpoint: "https://example.openai.azure.com/",
            apiKey: "fake-key");

        // Trigger factory execution by resolving the service
        var provider = result.BuildServiceProvider();
        _ = provider.GetService<Microsoft.Extensions.AI.IChatClient>();

        // Assert - verifies line 56 GetService<ILoggerFactory>() was called
        Assert.True(mockServiceProvider.GetServiceCalled);
        Assert.Equal(typeof(ILoggerFactory), mockServiceProvider.GetServiceType);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithLoggerFactoryAvailable_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddAzureOpenAIChatClient(
            deploymentName: "gpt-35-turbo",
            endpoint: "https://example.openai.azure.com/",
            apiKey: "fake-key",
            serviceId: "test");

        // Assert
        Assert.Same(services, result);
        var provider = result.BuildServiceProvider();
        var chatClient = provider.GetKeyedService<Microsoft.Extensions.AI.IChatClient>("test");
        Assert.NotNull(chatClient);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithoutServiceId_RegistersDefaultService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAzureOpenAIChatClient(
            deploymentName: "gpt-35-turbo",
            endpoint: "https://example.openai.azure.com/",
            apiKey: "fake-key");

        // Assert
        Assert.Same(services, result);
        var provider = result.BuildServiceProvider();
        var chatClient = provider.GetService<Microsoft.Extensions.AI.IChatClient>();
        Assert.NotNull(chatClient);
    }

    private class MockServiceProvider : IServiceProvider
    {
        public bool GetServiceCalled { get; private set; }
        public Type? GetServiceType { get; private set; }

        public object? GetService(Type serviceType)
        {
            GetServiceCalled = true;
            GetServiceType = serviceType;
            return NullLoggerFactory.Instance;
        }
    }
}
