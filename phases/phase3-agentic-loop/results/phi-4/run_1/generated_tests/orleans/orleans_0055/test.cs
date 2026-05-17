using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration.Overrides;
using Orleans.Runtime;
using Xunit;

public class OptionsOverridesTests
{
    [Fact]
    public void GetOverridableOption_WhenKeyedServiceFound_ReturnsCreatedOptions()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var expectedOption = new ClusterOptions();
        mockServiceProvider.Setup(s => s.GetKeyedService<ClusterOptions>("key")).Returns(expectedOption);

        // Act
        var result = OptionsOverrides.GetOverridableOption(mockServiceProvider.Object, "key");

        // Assert
        Assert.Same(expectedOption, result.Value);
    }

    [Fact]
    public void GetOverridableOption_WhenKeyedServiceNotFound_CallsGetRequiredService()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var expectedOptions = new Mock<IOptions<ClusterOptions>>();
        mockServiceProvider.Setup(s => s.GetKeyedService<ClusterOptions>("key")).Returns((ClusterOptions)null);
        mockServiceProvider.Setup(s => s.GetRequiredService<IOptions<ClusterOptions>>()).Returns(expectedOptions.Object);

        // Act
        var result = OptionsOverrides.GetOverridableOption(mockServiceProvider.Object, "key");

        // Assert
        Assert.Same(expectedOptions.Object, result);
        mockServiceProvider.Verify(s => s.GetRequiredService<IOptions<ClusterOptions>>(), Times.Once);
    }
}
