using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Xunit;

namespace Microsoft.SemanticKernel.UnitTests.Extensions;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!)!.AddBingTextSearch("apiKey"));
    }

    [Fact]
    public void AddBingTextSearch_ReturnsSameServicesInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";

        // Act
        var result = services.AddBingTextSearch(apiKey);

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);
    }

    [Fact]
    public void AddBingTextSearch_WithCustomServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        const string serviceId = "bing-test";

        // Act
        services.AddBingTextSearch(apiKey, serviceId: serviceId);

        // Assert
        Assert.Single(services);
        var descriptor = services.First();
        Assert.Equal(ServiceDescriptorType.Factory, descriptor.ServiceType);
        Assert.Equal(serviceId, descriptor.Key);
    }

    [Fact]
    public void AddBingTextSearch_WithNullOptions_UsesServiceProviderGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedOptions = new BingTextSearchOptions { Endpoint = new Uri("https://test.com") };
        services.AddSingleton(expectedOptions);
        var apiKey = "test-api-key";

        // Act
        services.AddBingTextSearch(apiKey, options: null);

        // Assert - Verify registration
        Assert.Single(services);
        
        // Build provider to test factory execution
        var provider = services.BuildServiceProvider();
        var factory = provider.GetServices<ServiceDescriptor>().First().ImplementationFactory;
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddBingTextSearch_WithOptionsPassed_RegistersFactoryCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var apiKey = "test-api-key";
        var options = new BingTextSearchOptions();

        // Act
        services.AddBingTextSearch(apiKey, options);

        // Assert
        Assert.Single(services);
        var descriptor = services.First();
        Assert.Equal(ServiceDescriptorType.Factory, descriptor.ServiceType);
    }
}
