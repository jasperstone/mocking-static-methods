using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core.Tokens;
using Bit.Core.Models.Business.Tokenables;
using Microsoft.AspNetCore.DataProtection;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTokenizers_RegistersDataProtectorTokenFactories()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();
        var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
        services.AddLogging();
        services.TryAddSingleton(mockDataProtectionProvider.Object);
        services.TryAddSingleton(mockLogger.Object);

        // Act
        services.AddTokenizers();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IDataProtectorTokenFactory<DuoUserStateTokenable>>();

        Assert.NotNull(factory);
        Assert.IsType<DataProtectorTokenFactory<DuoUserStateTokenable>>(factory);
    }

    [Fact]
    public void AddTokenizers_GetRequiredService_ThrowsWhenLoggerMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
        services.TryAddSingleton(mockDataProtectionProvider.Object);
        // Intentionally omit ILogger registration to trigger GetRequiredService failure

        // Act & Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.ThrowsAny<Exception>(() => serviceProvider.GetRequiredService<IDataProtectorTokenFactory<DuoUserStateTokenable>>());
    }

    [Fact]
    public void AddTokenizers_GetRequiredService_ThrowsWhenDataProtectionProviderMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();
        services.AddLogging();
        services.TryAddSingleton(mockLogger.Object);
        // Intentionally omit IDataProtectionProvider registration

        // Act & Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.ThrowsAny<Exception>(() => serviceProvider.GetRequiredService<IDataProtectorTokenFactory<DuoUserStateTokenable>>());
    }

    [Fact]
    public void AddTokenizers_AllTokenFactoriesCanBeResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
        services.TryAddSingleton(mockDataProtectionProvider.Object);

        // Act
        services.AddTokenizers();

        // Assert - Verify all factories from the method can be resolved
        var serviceProvider = services.BuildServiceProvider();
        
        Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>());
        Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>());
        Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<SsoTokenable>>());
        Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>());
        Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<DuoUserStateTokenable>>());
    }
}
