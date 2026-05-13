using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGlobalSettingsServices_DevelopmentSelfHostedTrue_BindsOverrideSection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new Mock<IConfiguration>();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.IsDevelopment()).Returns(true);

        var primarySection = new Mock<IConfigurationSection>();
        primarySection.Setup(s => s.Bind(It.IsAny<object>())).Verifiable();

        var overrideSection = new Mock<IConfigurationSection>();
        overrideSection.Setup(s => s.Bind(It.IsAny<object>())).Verifiable();

        configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(primarySection.Object);
        configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);
        configuration.Setup(c => c.GetSection("Dev:SelfHostOverride:GlobalSettings")).Returns(overrideSection.Object);

        // Act
        var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration.Object, environment.Object);

        // Assert
        primarySection.Verify(s => s.Bind(It.IsAny<object>()), Times.Once);
        overrideSection.Verify(s => s.Bind(It.IsAny<object>()), Times.Once);
        Assert.NotNull(result);
        Assert.Single(services.Where(d => d.ServiceType == typeof(GlobalSettings)));
        Assert.Single(services.Where(d => d.ServiceType == typeof(IGlobalSettings) && d.ImplementationType == typeof(GlobalSettings)));
    }

    [Fact]
    public void AddGlobalSettingsServices_DevelopmentSelfHostedFalse_DoesNotBindOverrideSection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new Mock<IConfiguration>();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.IsDevelopment()).Returns(true);

        var primarySection = new Mock<IConfigurationSection>();
        primarySection.Setup(s => s.Bind(It.IsAny<object>())).Verifiable();

        configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(primarySection.Object);
        configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(false);

        // Act
        var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration.Object, environment.Object);

        // Assert
        primarySection.Verify(s => s.Bind(It.IsAny<object>()), Times.Once);
        Assert.NotNull(result);
        Assert.Single(services.Where(d => d.ServiceType == typeof(GlobalSettings)));
        Assert.Single(services.Where(d => d.ServiceType == typeof(IGlobalSettings) && d.ImplementationType == typeof(GlobalSettings)));
    }

    [Fact]
    public void AddGlobalSettingsServices_NotDevelopment_DoesNotBindOverrideSection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new Mock<IConfiguration>();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.IsDevelopment()).Returns(false);

        var primarySection = new Mock<IConfigurationSection>();
        primarySection.Setup(s => s.Bind(It.IsAny<object>())).Verifiable();

        configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(primarySection.Object);
        configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

        // Act
        var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration.Object, environment.Object);

        // Assert
        primarySection.Verify(s => s.Bind(It.IsAny<object>()), Times.Once);
        Assert.NotNull(result);
        Assert.Single(services.Where(d => d.ServiceType == typeof(GlobalSettings)));
        Assert.Single(services.Where(d => d.ServiceType == typeof(IGlobalSettings) && d.ImplementationType == typeof(GlobalSettings)));
    }

    [Fact]
    public void AddGlobalSettingsServices_GetValueReturnsDefaultFalse_NoOverrideSectionBound()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new Mock<IConfiguration>();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.IsDevelopment()).Returns(true);

        var primarySection = new Mock<IConfigurationSection>();
        primarySection.Setup(s => s.Bind(It.IsAny<object>())).Verifiable();

        configuration.Setup(c => c.GetSection("GlobalSettings")).Returns(primarySection.Object);
        configuration.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(false);

        // Act
        var result = ServiceCollectionExtensions.AddGlobalSettingsServices(services, configuration.Object, environment.Object);

        // Assert
        primarySection.Verify(s => s.Bind(It.IsAny<object>()), Times.Once);
        Assert.NotNull(result);
    }
}
