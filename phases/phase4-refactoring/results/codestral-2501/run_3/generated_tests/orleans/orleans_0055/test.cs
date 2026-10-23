using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Core.Tests.Configuration
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_ReturnsNamedOption_WhenAvailable()
        {
            // Arrange
            var key = "namedOption";
            var namedOption = new Mock<ClusterOptions>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<ClusterOptions>(key))
                .Returns(namedOption.Object);

            // Act
            var result = OptionsOverridesWrapper.GetOverridableOption<ClusterOptions>(serviceProviderMock.Object, key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(namedOption.Object, result.Value);
        }

        [Fact]
        public void GetOverridableOption_ReturnsDefaultOption_WhenNamedOptionNotAvailable()
        {
            // Arrange
            var key = "namedOption";
            var defaultOption = new Mock<IOptions<ClusterOptions>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<ClusterOptions>(key))
                .Returns((ClusterOptions)null);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(defaultOption.Object);

            // Act
            var result = OptionsOverridesWrapper.GetOverridableOption<ClusterOptions>(serviceProviderMock.Object, key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(defaultOption.Object, result);
        }
    }

    public class ClusterOptions
    {
        // Dummy class for testing purposes
    }

    public static class OptionsOverridesWrapper
    {
        public static IOptions<TOptions> GetOverridableOption<TOptions>(IServiceProvider services, string key)
            where TOptions : class, new()
        {
            return OptionsOverrides.GetOverridableOption<TOptions>(services, key);
        }
    }
}
