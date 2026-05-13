using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using Xunit;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_ValidatesServicesNotNull()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddBingTextSearch("fake-api-key"));
    }

    [Fact]
    public void AddBingTextSearch_WithOptions_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new BingTextSearchOptions();
        var apiKey = "test-api-key";

        // Act
        services.AddBingTextSearch(apiKey, options);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetKeyedService<ITextSearch>(null);
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddBingTextSearch_NoOptions_UsesServiceProviderOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var registeredOptions = new BingTextSearchOptions();
        services.AddSingleton(registeredOptions);
        var apiKey = "test-api-key";

        // Act
        services.AddBingTextSearch(apiKey);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verifies the GetService<BingTextSearchOptions>() call executes without exception
        var factory = serviceProvider.GetKeyedService<ITextSearch>(null);
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddBraveTextSearch_UsesProvidedOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new BraveTextSearchOptions();
        var apiKey = "test-api-key";

        // Act
        services.AddBraveTextSearch(apiKey, options);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetKeyedService<ITextSearch>(null);
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddBraveTextSearch_NoOptions_UsesServiceProviderOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var registeredOptions = new BraveTextSearchOptions();
        services.AddSingleton(registeredOptions);
        var apiKey = "test-api-key";

        // Act
        services.AddBraveTextSearch(apiKey);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verifies the GetService<BraveTextSearchOptions>() call executes without exception
        var factory = serviceProvider.GetKeyedService<ITextSearch>(null);
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddGoogleTextSearch_ValidatesServicesNotNull()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddGoogleTextSearch("engine-id", "fake-api-key"));
    }

    [Fact]
    public void AddGoogleTextSearch_WithOptions_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new GoogleTextSearchOptions();
        var apiKey = "test-api-key";

        // Act
        services.AddGoogleTextSearch("engine-id", apiKey, options);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetKeyedService<ITextSearch>(null);
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddGoogleTextSearch_NoOptions_UsesServiceProviderOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var registeredOptions = new GoogleTextSearchOptions();
        services.AddSingleton(registeredOptions);
        var apiKey = "test-api-key";

        // Act
        services.AddGoogleTextSearch("engine-id", apiKey);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verifies the GetService<GoogleTextSearchOptions>() call executes without exception
        var factory = serviceProvider.GetKeyedService<ITextSearch>(null);
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddTavilyTextSearch_ValidatesServicesNotNull()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddTavilyTextSearch("fake-api-key"));
    }

    [Fact]
    public void AddTavilyTextSearch_WithOptions_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new TavilyTextSearchOptions();
        var apiKey = "test-api-key";

        // Act
        services.AddTavilyTextSearch(apiKey, options);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetKeyedService<ITextSearch>(null);
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddTavilyTextSearch_NoOptions_UsesServiceProviderOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var registeredOptions = new TavilyTextSearchOptions();
        services.AddSingleton(registeredOptions);
        var apiKey = "test-api-key";

        // Act
        services.AddTavilyTextSearch(apiKey);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verifies the GetService<TavilyTextSearchOptions>() call executes without exception
        var factory = serviceProvider.GetKeyedService<ITextSearch>(null);
        Assert.NotNull(factory);
    }
}
