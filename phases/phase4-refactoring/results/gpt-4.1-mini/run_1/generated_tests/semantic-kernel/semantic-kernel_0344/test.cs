using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Plugins.Web;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_WithOptions_RegistersServiceUsingProvidedOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var options = new BingTextSearchOptions();

        // Act
        services.AddBingTextSearch(apiKey, options);

        // Assert
        var provider = services.BuildServiceProvider();
        var factory = provider.GetService<Func<IServiceProvider, object?, object>>();
        Assert.NotNull(factory);
        var instance = factory(provider, null);
        Assert.NotNull(instance);
        Assert.IsType<BingTextSearch>(instance);
    }

    [Fact]
    public void AddBingTextSearch_WithoutOptions_ResolvesOptionsFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var options = new BingTextSearchOptions();
        services.AddSingleton(options);

        // Act
        services.AddBingTextSearch(apiKey);

        // Assert
        var provider = services.BuildServiceProvider();
        var factory = provider.GetService<Func<IServiceProvider, object?, object>>();
        Assert.NotNull(factory);
        var instance = factory(provider, null);
        Assert.NotNull(instance);
        Assert.IsType<BingTextSearch>(instance);
    }
}
