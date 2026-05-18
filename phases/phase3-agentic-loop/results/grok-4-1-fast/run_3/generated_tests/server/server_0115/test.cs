using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTokenizers_RegistersDuoUserStateTokenableFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();

        // Act
        services.AddTokenizers();

        // Assert - exercises the GetRequiredService call on line 204
        var serviceProvider = services.BuildServiceProvider();
        var duoFactory = serviceProvider.GetRequiredService<IDataProtectorTokenFactory<object>>();
        Assert.NotNull(duoFactory);
    }

    [Fact]
    public void AddTokenizers_RequiresLogging()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDataProtection();

        // Act & Assert
        services.AddTokenizers();
        var serviceProvider = services.BuildServiceProvider();
        
        Assert.ThrowsAny<InvalidOperationException>(() => 
            serviceProvider.GetRequiredService<IDataProtectorTokenFactory<object>>());
    }

    [Fact]
    public void AddTokenizers_RequiresDataProtection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert
        services.AddTokenizers();
        var serviceProvider = services.BuildServiceProvider();
        
        Assert.ThrowsAny<InvalidOperationException>(() => 
            serviceProvider.GetRequiredService<IDataProtectorTokenFactory<object>>());
    }
}
