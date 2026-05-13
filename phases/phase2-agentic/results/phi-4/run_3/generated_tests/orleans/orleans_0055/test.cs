using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_ReturnsNamedOption_WhenAvailable()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var namedOption = new ClusterOptions();
            mockServiceProvider.Setup(s => s.GetKeyedService<ClusterOptions>("namedKey")).Returns(namedOption);
            mockServiceProvider.Setup(s => s.GetRequiredService<IOptions<ClusterOptions>>()).Returns(Options.Create(new ClusterOptions()));

            // Act
            var result = OptionsOverrides.GetOverridableOption<ClusterOptions>(mockServiceProvider.Object, "namedKey");

            // Assert
            Assert.Same(namedOption, result.Value);
        }

        [Fact]
        public void GetOverridableOption_ReturnsDefaultOption_WhenNamedOptionNotAvailable()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var defaultOption = new ClusterOptions();
            mockServiceProvider.Setup(s => s.GetKeyedService<ClusterOptions>("nonExistentKey")).Returns((ClusterOptions)null);
            mockServiceProvider.Setup(s => s.GetRequiredService<IOptions<ClusterOptions>>()).Returns(Options.Create(defaultOption));

            // Act
            var result = OptionsOverrides.GetOverridableOption<ClusterOptions>(mockServiceProvider.Object, "nonExistentKey");

            // Assert
            Assert.Same(defaultOption, result.Value);
        }
    }
}
