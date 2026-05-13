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
            var namedOption = new ClusterOptions { SomeProperty = "NamedValue" };
            mockServiceProvider.Setup(sp => sp.GetKeyedService<ClusterOptions>("namedKey"))
                .Returns(namedOption);

            // Act
            var result = OptionsOverrides.GetOverridableOption(mockServiceProvider.Object, "namedKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(namedOption, result.Value);
        }

        [Fact]
        public void GetOverridableOption_ReturnsDefaultOption_WhenKeyedServiceDoesNotExist()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var defaultOption = new ClusterOptions { SomeProperty = "DefaultValue" };
            var optionsService = Options.Create(defaultOption);
            mockServiceProvider.Setup(sp => sp.GetKeyedService<ClusterOptions>("nonExistentKey"))
                .Returns((ClusterOptions)null);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(optionsService);

            // Act
            var result = OptionsOverrides.GetOverridableOption(mockServiceProvider.Object, "nonExistentKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(defaultOption, result.Value);
        }
    }
}
