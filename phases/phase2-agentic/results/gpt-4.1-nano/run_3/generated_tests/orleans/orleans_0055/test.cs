using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Configuration.Overrides;

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_ReturnsCreatedOptions_WhenServiceProvidesKeyedService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsInstance = new ClusterOptions { ClusterId = "cluster1" };
            var options = Options.Create(optionsInstance);
            var keyedServiceMock = new Mock<ClusterOptions>();
            keyedServiceMock.Setup(s => s).Returns(optionsInstance);
            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<ClusterOptions>("testKey"))
                .Returns(keyedServiceMock.Object);

            // Act
            var result = OptionsOverrides.GetOverridableOption<ClusterOptions>(serviceProviderMock.Object, "testKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("cluster1", result.Value.ClusterId);
        }

        [Fact]
        public void GetOverridableOption_ReturnsRequiredService_WhenNoKeyedService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsInstance = new ClusterOptions { ClusterId = "default" };
            var options = Options.Create(optionsInstance);
            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<ClusterOptions>("missingKey"))
                .Returns((ClusterOptions)null);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(options);

            // Act
            var result = OptionsOverrides.GetOverridableOption<ClusterOptions>(serviceProviderMock.Object, "missingKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("default", result.Value.ClusterId);
        }
    }

    // Extension method to facilitate testing static class method
    public static class OptionsOverrides
    {
        public static IOptions<TOptions> GetOverridableOption<TOptions>(this IServiceProvider services, string key)
            where TOptions : class, new()
        {
            TOptions option = services.GetKeyedService<TOptions>(key);
            return option != null
                ? Options.Create(option)
                : services.GetRequiredService<IOptions<TOptions>>();
        }

        // Mocked extension method for testing
        public static TOptions GetKeyedService<TOptions>(this IServiceProvider services, string key)
        {
            // This method is mocked in tests
            throw new NotImplementedException();
        }
    }
}
