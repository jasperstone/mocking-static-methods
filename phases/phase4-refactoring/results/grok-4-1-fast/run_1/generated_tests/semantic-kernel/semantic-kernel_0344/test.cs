using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Xunit;

namespace Microsoft.SemanticKernel.UnitTests.Extensions;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_WithOptions_RegistersKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-key";
        var options = new BingTextSearchOptions();

        // Act
        var result = services.AddBingTextSearch(apiKey, options);

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextSearch)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.NotNull(descriptor.ImplementationFactory);
    }

    [Fact]
    public void AddBingTextSearch_NullServices_ThrowsArgumentNullException()
    {
        Assert.ThrowsAny<ArgumentNullException>(() => ((IServiceCollection?)null!).AddBingTextSearch("key"));
    }

    [Fact]
    public void AddBingTextSearch_ResolvesWithProvidedOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<BingTextSearchOptions>(new BingTextSearchOptions());
        services.AddBingTextSearch("test-key", options: null);

        // Act
        using var provider = services.BuildServiceProvider();
        var textSearch = provider.GetKeyedService<ITextSearch>(null);

        // Assert - covers the sp.GetService<BingTextSearchOptions>() call on line 36
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddBingTextSearch_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceId = "bing-test";

        // Act
        services.AddBingTextSearch("test-key", serviceId: serviceId);

        // Assert
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextSearch)));
        // Keyed services have ImplementationFactory set
        Assert.NotNull(descriptor.ImplementationFactory);
    }

    [Fact]
    public void AddBraveTextSearch_RegistersKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-key";

        // Act
        var result = services.AddBraveTextSearch(apiKey);

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextSearch)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddGoogleTextSearch_RegistersKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var searchEngineId = "test-engine";
        var apiKey = "test-key";

        // Act
        var result = services.AddGoogleTextSearch(searchEngineId, apiKey);

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextSearch)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddTavilyTextSearch_RegistersKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-key";

        // Act
        var result = services.AddTavilyTextSearch(apiKey);

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextSearch)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }
}
