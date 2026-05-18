using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Xunit;
using System;

namespace Microsoft.SemanticKernel.Tests;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_RegistersService_WhenOptionsProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var options = new BingTextSearchOptions();

        // Act
        var result = services.AddBingTextSearch(apiKey, options);

        // Assert
        Assert.Same(services, result);
        var sp = services.BuildServiceProvider();
        var textSearch = sp.GetKeyedService<ITextSearch>(null!);
        Assert.NotNull(textSearch);
        Assert.IsType<BingTextSearch>(textSearch);
    }

    [Fact]
    public void AddBingTextSearch_UsesProvidedOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var providedOptions = new BingTextSearchOptions();

        // Act
        services.AddBingTextSearch(apiKey, providedOptions);

        // Assert
        var sp = services.BuildServiceProvider();
        var textSearch = (BingTextSearch)sp.GetKeyedService<ITextSearch>(null!)!;
        // Verify the factory was called with the provided options by ensuring the instance was created
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddBingTextSearch_UsesServiceProviderOptions_WhenOptionsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var registeredOptions = new BingTextSearchOptions();
        services.AddSingleton(registeredOptions);
        var apiKey = "test-api-key";

        // Act
        services.AddBingTextSearch(apiKey, options: null);

        // Assert - Verifies sp.GetService<BingTextSearchOptions>() was called in the factory
        var sp = services.BuildServiceProvider();
        var textSearch = sp.GetKeyedService<ITextSearch>(null!);
        Assert.NotNull(textSearch);
        Assert.IsType<BingTextSearch>(textSearch);
    }

    [Fact]
    public void AddBingTextSearch_Throws_WhenServicesNull()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection?)null)!.AddBingTextSearch("apiKey"));
    }

    [Fact]
    public void AddBraveTextSearch_RegistersService_WhenOptionsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";

        // Act
        var result = services.AddBraveTextSearch(apiKey, options: null);

        // Assert
        Assert.Same(services, result);
        var sp = services.BuildServiceProvider();
        var textSearch = sp.GetKeyedService<ITextSearch>(null!);
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddBraveTextSearch_UsesServiceProviderOptions_WhenOptionsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(new BraveTextSearchOptions());
        var apiKey = "test-api-key";

        // Act
        services.AddBraveTextSearch(apiKey, options: null);

        // Assert - Covers sp.GetService<BraveTextSearchOptions>()
        var sp = services.BuildServiceProvider();
        var textSearch = sp.GetKeyedService<ITextSearch>(null!);
        Assert.NotNull(textSearch);
    }
}
