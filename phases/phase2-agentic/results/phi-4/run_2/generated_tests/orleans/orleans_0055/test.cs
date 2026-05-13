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
        public void GetOverridableOption_ReturnsNamedOption_WhenKeyedServiceExists()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var namedOption = new ClusterOptions();
            mockServiceProvider
                .Setup(sp => sp.GetKeyedService<ClusterOptions>("testKey"))
                .Returns(namedOption);

            // Act
            var result = OptionsOverrides.GetOverridableOption<ClusterOptions>(mockServiceProvider.Object, "testKey");

            // Assert
            Assert.Same(namedOption, result.Value);
        }

        [Fact]
        public void GetOverridableOption_ReturnsDefaultOption_WhenKeyedServiceDoesNotExist()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var defaultOption = new ClusterOptions();
            var optionsProvider = Options.Create(defaultOption);
            mockServiceProvider
                .Setup(sp => sp.GetKeyedService<ClusterOptions>("nonExistentKey"))
                .Returns((ClusterOptions)null);
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(optionsProvider);

            // Act
            var result = OptionsOverrides.GetOverridableOption<ClusterOptions>(mockServiceProvider.Object, "nonExistentKey");

            // Assert
            Assert.Same(defaultOption, result.Value);
        }
    }
}
