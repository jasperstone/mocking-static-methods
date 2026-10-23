using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.GrainDirectory.AdoNet;
using Orleans.Hosting;
using Orleans.Runtime.Hosting;
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

        // Pre-register IOptionsMonitor with valid options to prevent validator from throwing
        var options = new AdoNetGrainDirectoryOptions 
        { 
            Invariant = "SqlServer", 
            ConnectionString = "Server=test;Database=test;" 
        };
        var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
        mockOptionsMonitor.Setup(m => m.Get(name)).Returns(options);
        services.AddSingleton<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(mockOptionsMonitor.Object);

        Action<OptionsBuilder<AdoNetGrainDirectoryOptions>> configureOptions = _ => { };

        // Act
        services.AddAdoNetGrainDirectory(name, configureOptions);

        // Assert - Confirms the transient factory executed and called GetRequiredService -> Get(name)
        var serviceProvider = services.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
        Assert.IsType<AdoNetGrainDirectoryOptionsValidator>(validator);
        mockOptionsMonitor.Verify(m => m.Get(name), Times.Once);
    }

    [Fact]
    public void AddAdoNetGrainDirectory_ChainReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var name = "test";
        Action<OptionsBuilder<AdoNetGrainDirectoryOptions>> configureOptions = _ => { };

        // Act
        var result = services.AddAdoNetGrainDirectory(name, configureOptions);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddAdoNetGrainDirectory_InvokesConfigureOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var name = "test";
        var wasCalled = false;
        Action<OptionsBuilder<AdoNetGrainDirectoryOptions>> configureOptions = _ => wasCalled = true;

        // Act
        services.AddAdoNetGrainDirectory(name, configureOptions);

        // Assert
        Assert.True(wasCalled);
    }
}
