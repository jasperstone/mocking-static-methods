using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using Xunit;

namespace Microsoft.SemanticKernel.UnitTests.Extensions;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_WhenCalled_ReturnsSameServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-key";

        // Act
        var result = services.AddBingTextSearch(apiKey);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddBingTextSearch_RegistersITextSearchKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-key";

        // Act
        services.AddBingTextSearch(apiKey);

        // Assert
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextSearch)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.NotNull(descriptor.ImplementationFactory);
        Assert.IsType<ServiceDescriptor>(descriptor);
    }

    [Fact]
    public void AddBingTextSearch_WithServiceId_SetsServiceKey()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceId = "bing-test";
        var apiKey = "test-key";

        // Act
        services.AddBingTextSearch(apiKey, serviceId: serviceId);

        // Assert
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextSearch)));
        Assert.Equal(serviceId, descriptor.ServiceKey);
    }

    [Fact]
    public void AddBingTextSearch_ThrowsOnNullServices()
    {
        Assert.ThrowsAny<ArgumentException>(() => WebServiceCollectionExtensions.AddBingTextSearch(null!, "key"));
    }

    [Fact]
    public void AddBraveTextSearch_WhenCalled_ReturnsSameServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-key";

        // Act
        var result = services.AddBraveTextSearch(apiKey);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddBraveTextSearch_RegistersITextSearchKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-key";

        // Act
        services.AddBraveTextSearch(apiKey);

        // Assert
        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITextSearch)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddGoogleTextSearch_WhenCalled_ReturnsSameServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var searchEngineId = "test-engine";
        var apiKey = "test-key";

        // Act
        var result = services.AddGoogleTextSearch(searchEngineId, apiKey);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddTavilyTextSearch_WhenCalled_ReturnsSameServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-key";

        // Act
        var result = services.AddTavilyTextSearch(apiKey);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddTavilyTextSearch_ThrowsOnNullServices()
    {
        Assert.ThrowsAny<ArgumentException>(() => WebServiceCollectionExtensions.AddTavilyTextSearch(null!, "key"));
    }
}
