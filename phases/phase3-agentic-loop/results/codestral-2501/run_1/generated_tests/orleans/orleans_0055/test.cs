using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Configuration;
using Moq;

public class OptionsOverridesTests
{
    [Fact]
    public void GetOverridableOption_ShouldReturnKeyedService_WhenKeyedServiceExists()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var keyedServiceMock = new Mock<ClusterOptions>();
        serviceProviderMock.Setup(sp => sp.GetKeyedService<ClusterOptions>("testKey")).Returns(keyedServiceMock.Object);

        // Act
        var result = OptionsOverrides.GetOverridableOption(serviceProviderMock.Object, "testKey");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(keyedServiceMock.Object, result.Value);
    }

    [Fact]
    public void GetOverridableOption_ShouldReturnRequiredService_WhenKeyedServiceDoesNotExist()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var requiredServiceMock = new Mock<IOptions<ClusterOptions>>();
        serviceProviderMock.Setup(sp => sp.GetKeyedService<ClusterOptions>("testKey")).Returns((ClusterOptions)null);
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>()).Returns(requiredServiceMock.Object);

        // Act
        var result = OptionsOverrides.GetOverridableOption(serviceProviderMock.Object, "testKey");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(requiredServiceMock.Object, result);
    }
}
