using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Xunit;

namespace Microsoft.SemanticKernel.Tests;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_WhenOptionsNull_UsesServiceProviderGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        var testOptions = new BingTextSearchOptions();
        services.AddSingleton(testOptions);

        // Act
        services.AddBingTextSearch("fake-api-key", options: null);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetRequiredKeyedService<ITextSearch>(null!);

        Assert.IsType<BingTextSearch>(textSearch);
    }

    [Fact]
    public void AddBingTextSearch_WhenOptionsProvided_UsesProvidedOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var providedOptions = new BingTextSearchOptions();

        // Act
        services.AddBingTextSearch("fake-api-key", options: providedOptions);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetRequiredKeyedService<ITextSearch>(null!);
        Assert.IsType<BingTextSearch>(textSearch);
    }

    [Fact]
    public void AddBingTextSearch_VerifyNotNullCalled()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ((IServiceCollection?)null!).AddBingTextSearch("fake-api-key"));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddBingTextSearch_ReturnsSameServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddBingTextSearch("fake-api-key");

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddBraveTextSearch_WhenOptionsNull_UsesServiceProviderGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<BraveTextSearchOptions>(new BraveTextSearchOptions());

        // Act
        services.AddBraveTextSearch("fake-api-key", options: null);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetRequiredKeyedService<ITextSearch>(null!);

        Assert.IsType<BraveTextSearch>(textSearch);
    }

    [Fact]
    public void AddBraveTextSearch_ReturnsSameServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddBraveTextSearch("fake-api-key");

        // Assert
        Assert.Same(services, result);
    }
}
