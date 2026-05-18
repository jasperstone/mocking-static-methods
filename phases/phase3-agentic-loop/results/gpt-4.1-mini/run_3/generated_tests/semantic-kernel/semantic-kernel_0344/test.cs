using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Xunit;

namespace Microsoft.SemanticKernel.Tests;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_UsesProvidedOptions_WhenOptionsNotNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var options = new Microsoft.SemanticKernel.Plugins.Web.Bing.BingTextSearchOptions { SomeOption = "value" };
        var serviceId = "bing";

        // Act
        WebServiceCollectionExtensions.AddBingTextSearch(services, apiKey, options, serviceId);

        // Assert
        var provider = services.BuildServiceProvider();
        var bing = provider.GetServiceByKey<ITextSearch>(serviceId);
        Assert.NotNull(bing);
        Assert.IsType<BingTextSearch>(bing);
        var bingTextSearch = (BingTextSearch)bing;
        Assert.Equal(apiKey, bingTextSearch.ApiKey);
        Assert.Equal(options, bingTextSearch.Options);
    }

    [Fact]
    public void AddBingTextSearch_UsesServiceProviderOptions_WhenOptionsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var serviceId = "bing";

        var options = new Microsoft.SemanticKernel.Plugins.Web.Bing.BingTextSearchOptions { SomeOption = "value" };
        services.AddSingleton(options);

        WebServiceCollectionExtensions.AddBingTextSearch(services, apiKey, null, serviceId);

        // Act
        var provider = services.BuildServiceProvider();
        var bing = provider.GetServiceByKey<ITextSearch>(serviceId);

        // Assert
        Assert.NotNull(bing);
        var bingTextSearch = Assert.IsType<BingTextSearch>(bing);
        Assert.Equal(apiKey, bingTextSearch.ApiKey);
        Assert.Equal(options, bingTextSearch.Options);
    }

    [Fact]
    public void AddBraveTextSearch_UsesProvidedOptions_WhenOptionsNotNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "brave-api-key";
        var options = new Microsoft.SemanticKernel.Plugins.Web.Brave.BraveTextSearchOptions { SomeOption = "value" };
        var serviceId = "brave";

        // Act
        WebServiceCollectionExtensions.AddBraveTextSearch(services, apiKey, options, serviceId);

        // Assert
        var provider = services.BuildServiceProvider();
        var brave = provider.GetServiceByKey<ITextSearch>(serviceId);
        Assert.NotNull(brave);
        Assert.IsType<BraveTextSearch>(brave);
        var braveTextSearch = (BraveTextSearch)brave;
        Assert.Equal(apiKey, braveTextSearch.ApiKey);
        Assert.Equal(options, braveTextSearch.Options);
    }

    [Fact]
    public void AddBraveTextSearch_UsesServiceProviderOptions_WhenOptionsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "brave-api-key";
        var serviceId = "brave";

        var options = new Microsoft.SemanticKernel.Plugins.Web.Brave.BraveTextSearchOptions { SomeOption = "value" };
        services.AddSingleton(options);

        WebServiceCollectionExtensions.AddBraveTextSearch(services, apiKey, null, serviceId);

        // Act
        var provider = services.BuildServiceProvider();
        var brave = provider.GetServiceByKey<ITextSearch>(serviceId);

        // Assert
        Assert.NotNull(brave);
        var braveTextSearch = Assert.IsType<BraveTextSearch>(brave);
        Assert.Equal(apiKey, braveTextSearch.ApiKey);
        Assert.Equal(options, braveTextSearch.Options);
    }
}

// Helper extension to resolve keyed services registered with AddKeyedSingleton
internal static class ServiceProviderExtensions
{
    public static T? GetServiceByKey<T>(this IServiceProvider provider, string? key)
    {
        if (key == null) return default;
        var keyedService = provider.GetService<KeyedService<T>>();
        if (keyedService == null) return default;
        return keyedService.GetService(key);
    }
}

// Dummy class to simulate keyed service container (since AddKeyedSingleton is not standard)
internal class KeyedService<T>
{
    public T? GetService(string key) => default;
}
