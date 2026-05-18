using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.GrainDirectory.AdoNet;
using Orleans.Hosting;
using Moq;
using Microsoft.Extensions.Options;
using Orleans.Runtime.Hosting;

public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAdoNetGrainDirectory_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var optionsBuilder = new Mock<OptionsBuilder<AdoNetGrainDirectoryOptions>>();
        var optionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
        var serviceProvider = new Mock<IServiceProvider>();

        serviceProvider.Setup(sp => sp.GetService(typeof(IOptionsMonitor<AdoNetGrainDirectoryOptions>)))
                       .Returns(optionsMonitor.Object);

        services.AddSingleton(serviceProvider.Object);

        // Act
        services.AddAdoNetGrainDirectory("Test", optionsBuilder.Object);

        // Assert
        var serviceProviderBuilt = services.BuildServiceProvider();
        var grainDirectory = serviceProviderBuilt.GetService<IConfigurationValidator>();
        Assert.NotNull(grainDirectory);
    }
}
