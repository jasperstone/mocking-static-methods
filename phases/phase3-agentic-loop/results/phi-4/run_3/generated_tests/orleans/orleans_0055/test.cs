using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration.Overrides;
using Xunit;

// Assuming ClusterOptions is a simple class for demonstration purposes
public class ClusterOptions
{
    // Add properties as needed
}

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_ReturnsNamedOption_WhenKeyedServiceExists()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var expectedOption = new ClusterOptions();
            mockServiceProvider
                .Setup(s => s.GetKeyedService<ClusterOptions>("testKey"))
                .Returns(expectedOption);

            // Act
            var result = OptionsOverrides.GetOverridableOption<ClusterOptions>(mockServiceProvider.Object, "testKey");

            // Assert
            Assert.Same(expectedOption, result.Value);
        }

        [Fact]
        public void GetOverridableOption_ReturnsDefaultOption_WhenKeyedServiceDoesNotExist()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var defaultOption = new ClusterOptions();
            var mockOptions = Options.Create(defaultOption);
            mockServiceProvider
                .Setup(s => s.GetKeyedService<ClusterOptions>("testKey"))
                .Returns((ClusterOptions)null);
            mockServiceProvider
                .Setup(s => s.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(mockOptions);

            // Act
            var result = OptionsOverrides.GetOverridableOption<ClusterOptions>(mockServiceProvider.Object, "testKey");

            // Assert
            Assert.Same(defaultOption, result.Value);
        }
    }
}
