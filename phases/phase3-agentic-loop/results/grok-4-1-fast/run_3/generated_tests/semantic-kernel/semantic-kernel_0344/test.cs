using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Brave;
using Microsoft.SemanticKernel.Data;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Plugins.Web;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_WhenCalledWithNullServices_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection?)null!).AddBingTextSearch("fake-api-key"));
    }

    [Fact]
    public void AddBingTextSearch_WhenOptionsNullAndNoServiceRegistered_SucceedsWithNullOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddBingTextSearch("fake-api-key");

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);
        
        var descriptor = services.First();
        Assert.Equal(ServiceDescriptor.ServiceType, descriptor.ServiceType);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddBingTextSearch_WhenOptionsProvided_RegistersFactorySuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new BingTextSearchOptions();

        // Act
        var result = services.AddBingTextSearch("fake-api-key", options);

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);
    }

    [Fact]
    public void AddBingTextSearch_WhenOptionsNullButServiceRegistered_UsesRegisteredOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var registeredOptions = new BingTextSearchOptions();
        services.AddSingleton(registeredOptions);

        // Act
        var result = services.AddBingTextSearch("fake-api-key");

        // Assert
        Assert.Same(services, result);
        Assert.Equal(2, services.Count());
    }

    [Fact]
    public void AddBraveTextSearch_WhenOptionsNullAndNoServiceRegistered_SucceedsWithNullOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddBraveTextSearch("fake-api-key");

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);
    }

    [Fact]
    public void AddBraveTextSearch_WhenOptionsNullButServiceRegistered_UsesRegisteredOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var registeredOptions = new BraveTextSearchOptions();
        services.AddSingleton(registeredOptions);

        // Act
        var result = services.AddBraveTextSearch("fake-api-key");

        // Assert
        Assert.Same(services, result);
        Assert.Equal(2, services.Count());
    }
}
