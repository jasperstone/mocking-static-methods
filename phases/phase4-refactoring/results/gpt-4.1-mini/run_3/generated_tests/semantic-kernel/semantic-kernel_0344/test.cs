using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel;
using Xunit;

namespace Microsoft.SemanticKernel.Tests;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_RegistersService_WithProvidedOptions()
    {
        var services = new ServiceCollection();
        var options = new BingTextSearchOptions();
        string apiKey = "test-api-key";
        string serviceId = "bing";

        services.AddBingTextSearch(apiKey, options, serviceId);

        var provider = services.BuildServiceProvider();
        var textSearch = provider.GetService<ITextSearch>();

        Assert.NotNull(textSearch);
        Assert.IsType<BingTextSearch>(textSearch);
    }

    [Fact]
    public void AddBingTextSearch_UsesServiceProviderOptions_WhenOptionsNotProvided()
    {
        var services = new ServiceCollection();
        var options = new BingTextSearchOptions();
        services.AddSingleton(options);
        string apiKey = "test-api-key";
        string serviceId = "bing";

        services.AddBingTextSearch(apiKey, null, serviceId);

        var provider = services.BuildServiceProvider();
        var textSearch = provider.GetService<ITextSearch>();

        Assert.NotNull(textSearch);
        Assert.IsType<BingTextSearch>(textSearch);
    }

    [Fact]
    public void AddBraveTextSearch_RegistersService_WithProvidedOptions()
    {
        var services = new ServiceCollection();
        var options = new BraveTextSearchOptions();
        string apiKey = "test-api-key";
        string serviceId = "brave";

        services.AddBraveTextSearch(apiKey, options, serviceId);

        var provider = services.BuildServiceProvider();
        var textSearch = provider.GetService<ITextSearch>();

        Assert.NotNull(textSearch);
        Assert.IsType<BraveTextSearch>(textSearch);
    }

    [Fact]
    public void AddBraveTextSearch_UsesServiceProviderOptions_WhenOptionsNotProvided()
    {
        var services = new ServiceCollection();
        var options = new BraveTextSearchOptions();
        services.AddSingleton(options);
        string apiKey = "test-api-key";
        string serviceId = "brave";

        services.AddBraveTextSearch(apiKey, null, serviceId);

        var provider = services.BuildServiceProvider();
        var textSearch = provider.GetService<ITextSearch>();

        Assert.NotNull(textSearch);
        Assert.IsType<BraveTextSearch>(textSearch);
    }
}
