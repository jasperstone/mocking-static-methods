using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Web.Tests;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_WithOptions_RegistersBingTextSearch()
    {
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var options = new BingTextSearchOptions();

        services.AddBingTextSearch(apiKey, options);

        var factory = GetFactoryFromServiceCollection(services);
        Assert.NotNull(factory);

        var spWithOptions = new ServiceCollection()
            .AddSingleton(options)
            .BuildServiceProvider();

        var instance = factory(spWithOptions, null);
        Assert.NotNull(instance);
        Assert.IsType<BingTextSearch>(instance);
    }

    [Fact]
    public void AddBingTextSearch_WithoutOptions_UsesServiceProviderOptions()
    {
        var services = new ServiceCollection();
        var apiKey = "test-api-key";

        services.AddBingTextSearch(apiKey);

        var factory = GetFactoryFromServiceCollection(services);
        Assert.NotNull(factory);

        var options = new BingTextSearchOptions();
        var spWithOptions = new ServiceCollection()
            .AddSingleton(options)
            .BuildServiceProvider();

        var instance = factory(spWithOptions, null);
        Assert.NotNull(instance);
        Assert.IsType<BingTextSearch>(instance);
    }

    [Fact]
    public void AddBraveTextSearch_WithOptions_RegistersBraveTextSearch()
    {
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var options = new BraveTextSearchOptions();

        services.AddBraveTextSearch(apiKey, options);

        var factory = GetFactoryFromServiceCollection(services);
        Assert.NotNull(factory);

        var spWithOptions = new ServiceCollection()
            .AddSingleton(options)
            .BuildServiceProvider();

        var instance = factory(spWithOptions, null);
        Assert.NotNull(instance);
        Assert.IsType<BraveTextSearch>(instance);
    }

    [Fact]
    public void AddBraveTextSearch_WithoutOptions_UsesServiceProviderOptions()
    {
        var services = new ServiceCollection();
        var apiKey = "test-api-key";

        services.AddBraveTextSearch(apiKey);

        var factory = GetFactoryFromServiceCollection(services);
        Assert.NotNull(factory);

        var options = new BraveTextSearchOptions();
        var spWithOptions = new ServiceCollection()
            .AddSingleton(options)
            .BuildServiceProvider();

        var instance = factory(spWithOptions, null);
        Assert.NotNull(instance);
        Assert.IsType<BraveTextSearch>(instance);
    }

    private static Func<IServiceProvider, object?, object>? GetFactoryFromServiceCollection(IServiceCollection services)
    {
        foreach (var descriptor in services)
        {
            if (descriptor.ImplementationFactory != null)
            {
                var factory = descriptor.ImplementationFactory;
                return (sp, obj) => factory(sp);
            }
        }
        return null;
    }
}
