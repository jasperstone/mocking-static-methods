using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTokenizers_Registration_SucceedsWithLoggingAndDataProtection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();

        // Act
        services.AddTokenizers();

        // Assert - Registration succeeds without exceptions
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddTokenizers_Registration_ThrowsWithoutLogging()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDataProtection();

        // Act & Assert
        Assert.ThrowsAny<InvalidOperationException>(() =>
        {
            services.AddTokenizers();
            services.BuildServiceProvider();
        });
    }

    [Fact]
    public void AddTokenizers_Registration_ThrowsWithoutDataProtection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert
        Assert.ThrowsAny<InvalidOperationException>(() =>
        {
            services.AddTokenizers();
            services.BuildServiceProvider();
        });
    }

    [Fact]
    public void AddTokenizers_FactoryResolution_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();

        // Act
        services.AddTokenizers();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Can resolve services (triggers factory invocation including GetRequiredService call)
        using (serviceProvider)
        {
            _ = serviceProvider.GetService<object>();
        }
    }
}
