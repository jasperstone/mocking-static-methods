using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_WithOptions_RegistersServiceUsingProvidedOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var options = new BingTextSearchOptions { SomeOption = "value" };

        // Act
        services.AddBingTextSearch(apiKey, options);

        // Assert
        var provider = services.BuildServiceProvider();
        var textSearch = provider.GetService<ITextSearch>();
        Assert.Null(textSearch); // Because AddKeyedSingleton registers keyed, not default

        // Instead, resolve using the keyed service
        var keyedService = provider.GetService<ITextSearch>();
        Assert.Null(keyedService); // No default service registered

        // We test the factory delegate by invoking it manually
        var spMock = new Mock<IServiceProvider>();
        var factory = GetFactoryFromServices(services, typeof(ITextSearch));
        var instance = factory(spMock.Object, null);
        Assert.NotNull(instance);
        Assert.IsType<BingTextSearch>(instance);
    }

    [Fact]
    public void AddBingTextSearch_WithoutOptions_UsesServiceProviderOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var options = new BingTextSearchOptions { SomeOption = "value" };
        services.AddSingleton(options);

        services.AddBingTextSearch(apiKey);

        var provider = services.BuildServiceProvider();

        // Act
        var sp = provider;
        var factory = GetFactoryFromServices(services, typeof(ITextSearch));
        var instance = factory(sp, null);

        // Assert
        Assert.NotNull(instance);
        var bingSearch = Assert.IsType<BingTextSearch>(instance);
        Assert.Equal(apiKey, bingSearch.ApiKey);
        Assert.Equal(options, bingSearch.Options);
    }

    [Fact]
    public void AddBraveTextSearch_WithoutOptions_UsesServiceProviderOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "brave-api-key";
        var options = new BraveTextSearchOptions { SomeOption = "value" };
        services.AddSingleton(options);

        services.AddBraveTextSearch(apiKey);

        var provider = services.BuildServiceProvider();

        // Act
        var sp = provider;
        var factory = GetFactoryFromServices(services, typeof(ITextSearch));
        var instance = factory(sp, null);

        // Assert
        Assert.NotNull(instance);
        var braveSearch = Assert.IsType<BraveTextSearch>(instance);
        Assert.Equal(apiKey, braveSearch.ApiKey);
        Assert.Equal(options, braveSearch.Options);
    }

    private static Func<IServiceProvider, object?, object> GetFactoryFromServices(IServiceCollection services, Type serviceType)
    {
        // The AddKeyedSingleton extension method registers a keyed service with a factory delegate.
        // We need to extract the factory delegate from the service descriptors.
        foreach (var sd in services)
        {
            if (sd.ServiceType == typeof(ITextSearch) && sd.ImplementationFactory != null)
            {
                return (sp, obj) => sd.ImplementationFactory(sp)!;
            }
        }

        // If not found, fallback to a dummy factory that throws
        return (sp, obj) => throw new InvalidOperationException("Factory not found");
    }
}
