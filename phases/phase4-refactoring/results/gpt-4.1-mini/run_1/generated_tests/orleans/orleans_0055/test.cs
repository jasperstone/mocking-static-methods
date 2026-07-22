using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration.Overrides;
using Orleans.Configuration;
using Xunit;

namespace Orleans.Core.Configuration.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetProviderClusterOptions_ReturnsOptionsFromGetRequiredService_WhenKeyedServiceThrows()
        {
            // Arrange
            var providerName = "testProvider";

            var clusterOptions = new ClusterOptions { ClusterId = "cluster2", ServiceId = "service2" };
            var optionsMock = Options.Create(clusterOptions);

            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup GetKeyedService<T> to throw InvalidOperationException to simulate unsupported keyed services
            serviceProviderMock.Setup(sp => Microsoft.Extensions.DependencyInjection.ServiceProviderKeyedServiceExtensions.GetKeyedService<ClusterOptions>(serviceProviderMock.Object, providerName))
                .Throws(new InvalidOperationException("This service provider doesn't support keyed services."));

            // Setup GetRequiredService<IOptions<ClusterOptions>>() to return optionsMock
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>()).Returns(optionsMock);

            // Act
            var options = OptionsOverrides.GetProviderClusterOptions(serviceProviderMock.Object, providerName);

            // Assert
            Assert.NotNull(options);
            Assert.Equal(clusterOptions.ClusterId, options.Value.ClusterId);
            Assert.Equal(clusterOptions.ServiceId, options.Value.ServiceId);
        }
    }
}
