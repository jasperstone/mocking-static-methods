using System;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_ReturnsOption_WhenKeyedServiceExists()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var expectedOptions = new ClusterOptions { ServiceId = "test" };
            serviceCollection.AddSingleton<IOptions<ClusterOptions>>(Options.Create(expectedOptions));
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetOverridableOption<ClusterOptions>("anyKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOptions.ServiceId, result.Value.ServiceId);
        }

        [Fact]
        public void GetOverridableOption_ReturnsRequiredService_WhenKeyedServiceDoesNotExist()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions { ServiceId = "default" }));
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            var result = serviceProvider.GetOverridableOption<ClusterOptions>("nonexistentKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("default", result.Value.ServiceId);
        }

        [Fact]
        public void GetProviderClusterOptions_CallsGetOverridableOption()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var providerName = "provider1";

            var optionsMock = new Mock<IOptions<ClusterOptions>>();
            var clusterOptions = new ClusterOptions { ServiceId = "overridden" };
            optionsMock.Setup(o => o.Value).Returns(clusterOptions);

            servicesMock.Setup(s => s.GetOverridableOption<ClusterOptions>(providerName))
                .Returns(optionsMock.Object);

            // Act
            var result = servicesMock.Object.GetProviderClusterOptions(providerName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("overridden", result.Value.ServiceId);
        }
    }
}
