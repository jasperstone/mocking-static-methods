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
    public void AddBingTextSearch_WithOptions_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var options = new BingTextSearchOptions { Endpoint = new Uri("https://example.com") };

        var result = services.AddBingTextSearch(apiKey, options);

        Assert.Same(services, result);
    }

    [Fact]
    public void AddBingTextSearch_WithoutOptions_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var apiKey = "test-api-key";

        services.AddSingleton(new BingTextSearchOptions { Endpoint = new Uri("https://example.com") });

        var result = services.AddBingTextSearch(apiKey);

        Assert.Same(services, result);
    }

    [Fact]
    public void AddBraveTextSearch_WithOptions_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var apiKey = "brave-api-key";
        var options = new BraveTextSearchOptions();

        var result = services.AddBraveTextSearch(apiKey, options);

        Assert.Same(services, result);
    }

    [Fact]
    public void AddBraveTextSearch_WithoutOptions_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var apiKey = "brave-api-key";

        services.AddSingleton(new BraveTextSearchOptions());

        var result = services.AddBraveTextSearch(apiKey);

        Assert.Same(services, result);
    }
}
