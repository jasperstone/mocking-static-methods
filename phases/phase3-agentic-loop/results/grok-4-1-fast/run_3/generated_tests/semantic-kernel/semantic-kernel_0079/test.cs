using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class AzureOpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureOpenAIChatClient_CallsGetServiceForILoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockServiceProvider = new MockServiceProvider();
        services.AddSingleton<IServiceProvider>(mockServiceProvider);

        // Act
        _ = services.AddAzureOpenAIChatClient(
            deploymentName: "gpt-35-turbo",
            endpoint: "https://example.openai.azure.com/",
            apiKey: "fake-key");

        // Trigger the factory to execute GetService call
        var serviceProvider = services.BuildServiceProvider();
        _ = serviceProvider.GetService<object>(serviceKey: "default");

        // Assert
        Assert.True(mockServiceProvider.GetServiceCalled);
        Assert.Equal(typeof(ILoggerFactory), mockServiceProvider.GetServiceType);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithLoggerFactoryAvailable_RegistersChatClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddAzureOpenAIChatClient(
            deploymentName: "gpt-35-turbo",
            endpoint: "https://example.openai.azure.com/",
            apiKey: "fake-key");

        // Assert
        Assert.Same(services, result);
        var serviceProvider = result.BuildServiceProvider();
        var chatClient = serviceProvider.GetService<object>(serviceKey: "default");
        Assert.NotNull(chatClient);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithoutLoggerFactory_RegistersChatClient()
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
        var serviceProvider = result.BuildServiceProvider();
        var chatClient = serviceProvider.GetService<object>(serviceKey: "default");
        Assert.NotNull(chatClient);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_ValidatesParameters()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddAzureOpenAIChatClient("deploy", null!, "key"));
        Assert.Throws<ArgumentException>(() => services.AddAzureOpenAIChatClient("deploy", "", "key"));
        Assert.Throws<ArgumentNullException>(() => services.AddAzureOpenAIChatClient(null!, "endpoint", "key"));
        Assert.Throws<ArgumentException>(() => services.AddAzureOpenAIChatClient("", "endpoint", "key"));
    }

    private class MockServiceProvider : IServiceProvider, IServiceScope, IDisposable
    {
        public bool GetServiceCalled { get; private set; }
        public Type? GetServiceType { get; private set; }

        public object? GetService(Type serviceType)
        {
            GetServiceCalled = true;
            GetServiceType = serviceType;
            return serviceType == typeof(ILoggerFactory) ? NullLoggerFactory.Instance : null;
        }

        public object? GetService(Type serviceType, object? serviceKey) => GetService(serviceType);

        IServiceProvider IServiceScope.ServiceProvider => this;
        public IServiceScope CreateScope() => this;
        public void Dispose() { }
    }
}
