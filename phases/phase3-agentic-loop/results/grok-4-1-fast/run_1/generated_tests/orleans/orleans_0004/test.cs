using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Hosting;
using System;
using Xunit;

namespace Orleans.Hosting.Tests;

public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAdoNetGrainDirectory_RegistersValidatorUsingGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        var name = "test";

        // Mock IOptionsMonitor to return valid options
        var options = new Mock<AdoNetGrainDirectoryOptions>();
        options.SetupGet(o => o.Invariant).Returns("SqlServer");
        options.SetupGet(o => o.ConnectionString).Returns("valid connection string");
        var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
        optionsMonitorMock.Setup(m => m.Get(name)).Returns(options.Object);
        services.AddSingleton<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(optionsMonitorMock.Object);

        // Mock configureOptions
        var configureOptionsMock = new Mock<Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>>();

        // Act
        var result = AdoNetGrainDirectoryServiceCollectionExtensions.AddAdoNetGrainDirectory(services, name, configureOptionsMock.Object);

        // Assert
        Assert.Same(services, result);
        
        // Verify validator registration
        var validatorDescriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IConfigurationValidator)));
        Assert.Equal(ServiceLifetime.Transient, validatorDescriptor.Lifetime);

        // Verify factory creates validator using GetRequiredService by building SP
        var serviceProvider = services.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
        Assert.IsType<AdoNetGrainDirectoryOptionsValidator>(validator);
    }

    [Fact]
    public void AddAdoNetGrainDirectory_ThrowsWhenOptionsMonitorMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var name = "test";
        var configureOptionsMock = new Mock<Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>>();

        // Act
        AdoNetGrainDirectoryServiceCollectionExtensions.AddAdoNetGrainDirectory(services, name, configureOptionsMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - should throw InvalidOperationException from GetRequiredService
        var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IConfigurationValidator>());
        Assert.Contains("GetRequiredService", exception.Message);
    }
}
