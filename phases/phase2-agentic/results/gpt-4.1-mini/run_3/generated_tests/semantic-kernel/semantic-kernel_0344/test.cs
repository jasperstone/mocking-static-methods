using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Tests;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_UsesProvidedOptions_WhenOptionsAreNotNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var options = new BingTextSearchOptions { SomeOption = "value" };
        var serviceId = "bing";

        // Act
        services.AddBingTextSearch(apiKey, options, serviceId);

        // Assert
        var provider = services.BuildServiceProvider();
        var textSearch = provider.GetService<ITextSearch>();
        Assert.Null(textSearch); // Because AddKeyedSingleton registers keyed, not default

        // Instead, test the factory delegate by invoking it manually
        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(ITextSearch));
        var factory = (Func<IServiceProvider, object, ITextSearch>)descriptor.ImplementationFactory!;
        var result = factory(provider, null);
        Assert.NotNull(result);
        Assert.IsType<BingTextSearch>(result);
        var bing = (BingTextSearch)result;
        Assert.Equal(apiKey, bing.ApiKey);
        Assert.Equal(options, bing.Options);
    }

    [Fact]
    public void AddBingTextSearch_UsesServiceProviderOptions_WhenOptionsAreNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var serviceId = "bing";

        var options = new BingTextSearchOptions { SomeOption = "value" };
        services.AddSingleton(options);

        // Act
        services.AddBingTextSearch(apiKey, null, serviceId);

        // Assert
        var provider = services.BuildServiceProvider();

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(ITextSearch));
        var factory = (Func<IServiceProvider, object, ITextSearch>)descriptor.ImplementationFactory!;
        var result = factory(provider, null);
        Assert.NotNull(result);
        Assert.IsType<BingTextSearch>(result);
        var bing = (BingTextSearch)result;
        Assert.Equal(apiKey, bing.ApiKey);
        Assert.Equal(options, bing.Options);
    }

    [Fact]
    public void AddBraveTextSearch_UsesProvidedOptions_WhenOptionsAreNotNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var options = new BraveTextSearchOptions { SomeOption = "value" };
        var serviceId = "brave";

        // Act
        services.AddBraveTextSearch(apiKey, options, serviceId);

        // Assert
        var provider = services.BuildServiceProvider();

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(ITextSearch));
        var factory = (Func<IServiceProvider, object, ITextSearch>)descriptor.ImplementationFactory!;
        var result = factory(provider, null);
        Assert.NotNull(result);
        Assert.IsType<BraveTextSearch>(result);
        var brave = (BraveTextSearch)result;
        Assert.Equal(apiKey, brave.ApiKey);
        Assert.Equal(options, brave.Options);
    }

    [Fact]
    public void AddBraveTextSearch_UsesServiceProviderOptions_WhenOptionsAreNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var serviceId = "brave";

        var options = new BraveTextSearchOptions { SomeOption = "value" };
        services.AddSingleton(options);

        // Act
        services.AddBraveTextSearch(apiKey, null, serviceId);

        // Assert
        var provider = services.BuildServiceProvider();

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(ITextSearch));
        var factory = (Func<IServiceProvider, object, ITextSearch>)descriptor.ImplementationFactory!;
        var result = factory(provider, null);
        Assert.NotNull(result);
        Assert.IsType<BraveTextSearch>(result);
        var brave = (BraveTextSearch)result;
        Assert.Equal(apiKey, brave.ApiKey);
        Assert.Equal(options, brave.Options);
    }
}
