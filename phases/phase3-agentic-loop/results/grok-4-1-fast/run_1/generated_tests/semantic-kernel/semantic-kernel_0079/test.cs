using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class AzureOpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureOpenAIChatClient_WithApiKey_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        var mockServiceProvider = new MockServiceProvider();
        services.AddSingleton<IServiceProvider>(mockServiceProvider);

        // Act
        services.AddAzureOpenAIChatClient(
            deploymentName: "test-deployment",
            endpoint: "https://test-endpoint.openai.azure.com",
            apiKey: "test-api-key");

        // Trigger factory execution to call GetService
        using var serviceProvider = services.BuildServiceProvider();
        _ = serviceProvider.GetRequiredService<object>();

        // Assert
        Assert.True(mockServiceProvider.GetServiceCalled);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithoutLoggerFactory_ReturnsValidServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAzureOpenAIChatClient(
            deploymentName: "test-deployment",
            endpoint: "https://test-endpoint.openai.azure.com",
            apiKey: "test-api-key");

        // Assert
        Assert.Same(services, result);
    }

    private class MockServiceProvider : IServiceProvider
    {
        public bool GetServiceCalled { get; private set; }

        public object? GetService(Type serviceType)
        {
            GetServiceCalled = true;
            if (serviceType == typeof(ILoggerFactory))
            {
                return NullLoggerFactory.Instance;
            }
            return null;
        }
    }
}
