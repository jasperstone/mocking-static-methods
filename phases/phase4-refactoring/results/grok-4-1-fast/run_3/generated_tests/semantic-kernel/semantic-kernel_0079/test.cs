using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class AzureOpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureOpenAIChatClient_WithApiKey_RegistersKeyedSingletonFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddAzureOpenAIChatClient(
            deploymentName: "gpt-35-turbo",
            endpoint: "https://example.openai.azure.com/",
            apiKey: "fake-key");

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IChatClient)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Null(descriptor.ServiceKey);
        Assert.IsType<ServiceDescriptor>(descriptor);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithServiceId_RegistersKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        const string serviceId = "test-service";

        // Act
        services.AddAzureOpenAIChatClient(
            deploymentName: "gpt-35-turbo",
            endpoint: "https://example.openai.azure.com/",
            apiKey: "fake-key",
            serviceId: serviceId);

        // Assert
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IChatClient)));
        Assert.Equal(serviceId, descriptor.ServiceKey);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithoutLoggerFactory_StillRegisters()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var result = services.AddAzureOpenAIChatClient(
            deploymentName: "gpt-35-turbo",
            endpoint: "https://example.openai.azure.com/",
            apiKey: "fake-key");

        Assert.Same(services, result);
        Assert.Single(services);
    }

    [Fact]
    public void AddAzureOpenAIChatClient_WithTokenCredential_RegistersFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var fakeCredential = new FakeTokenCredential();

        // Act
        var result = services.AddAzureOpenAIChatClient(
            deploymentName: "gpt-35-turbo",
            endpoint: "https://example.openai.azure.com/",
            credentials: fakeCredential);

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IChatClient)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    private sealed class FakeTokenCredential : Azure.Core.TokenCredential
    {
        public override AccessToken GetToken(
            Azure.Core.TokenRequestContext requestContext,
            System.Threading.CancellationToken cancellationToken) =>
            new AccessToken("fake-token", DateTimeOffset.UtcNow.AddDays(1));
    }
}
