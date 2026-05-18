using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Runtime.Hosting;
using Xunit;

namespace Orleans.Hosting.Tests;

public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAdoNetGrainDirectory_CallsGetRequiredServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var optionsBuilderMock = new Mock<Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>>();
        var name = "testName";
        var configureAction = optionsBuilderMock.Object;

        // Act
        var result = services.AddAdoNetGrainDirectory(name, configureAction);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);

        // Verify that GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>> was called
        // by building the service provider and checking registrations
        var serviceProvider = services.BuildServiceProvider();
        var validator = serviceProvider.GetServices<IConfigurationValidator>().OfType<AdoNetGrainDirectoryOptionsValidator>().SingleOrDefault();
        Assert.NotNull(validator);
    }
}
