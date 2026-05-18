using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Xunit;

namespace Orleans.GrainDirectory.AdoNet.Tests;

public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAdoNetGrainDirectory_RegistersValidatorUsingGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        var name = "test";

        var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
        var expectedOptions = new AdoNetGrainDirectoryOptions 
        { 
            Invariant = "SqlServer", 
            ConnectionString = "Server=.;Database=test;Trusted_Connection=true;" 
        };
        optionsMonitorMock.Setup(m => m.Get(name)).Returns(expectedOptions);
        services.AddSingleton<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(optionsMonitorMock.Object);

        int getRequiredServiceCalls = 0;
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>())
            .Callback(() => getRequiredServiceCalls++)
            .Returns(optionsMonitorMock.Object);
        services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

        Action<OptionsBuilder<AdoNetGrainDirectoryOptions>> configureOptions = _ => { };

        // Act
        var result = Orleans.Hosting.AdoNetGrainDirectoryServiceCollectionExtensions.AddAdoNetGrainDirectory(services, name, configureOptions);

        // Assert
        Assert.Equal(1, getRequiredServiceCalls);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(), Times.Once());
        Assert.NotNull(result);
    }

    [Fact]
    public void AddAdoNetGrainDirectory_ValidatorReceivesCorrectParameters()
    {
        // Arrange
        var services = new ServiceCollection();
        var name = "test";
        var expectedOptions = new AdoNetGrainDirectoryOptions 
        { 
            Invariant = "SqlServer", 
            ConnectionString = "Server=.;Database=test;Trusted_Connection=true;" 
        };

        var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
        optionsMonitorMock.Setup(m => m.Get(name)).Returns(expectedOptions);
        services.AddSingleton<IOptionsMonitor<AdoNetGrainDirectoryOptions>>(optionsMonitorMock.Object);

        AdoNetGrainDirectoryOptionsValidator? createdValidator = null;
        services.AddTransient<IConfigurationValidator>(sp =>
        {
            var actualOptions = sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>().Get(name);
            createdValidator = new Orleans.Configuration.AdoNetGrainDirectoryOptionsValidator(actualOptions, name);
            return createdValidator;
        });

        Action<OptionsBuilder<AdoNetGrainDirectoryOptions>> configureOptions = _ => { };

        // Act
        _ = Orleans.Hosting.AdoNetGrainDirectoryServiceCollectionExtensions.AddAdoNetGrainDirectory(services, name, configureOptions);
        var provider = services.BuildServiceProvider();
        _ = provider.GetRequiredService<IConfigurationValidator>();

        // Assert
        Assert.NotNull(createdValidator);
        // Test via validation behavior since fields are private
        createdValidator.ValidateConfiguration(); // Should not throw with valid options
    }
}
