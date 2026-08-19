using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Storage;
using Xunit;

namespace Orleans.Storage.Tests
{
    public static class ServiceProviderExtensions
    {
        public static IOptions<ClusterOptions> GetProviderClusterOptions(this IServiceProvider provider, string name)
        {
            return new Mock<IOptions<ClusterOptions>>().Object;
        }
    }

    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnAzureTableGrainStorage()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            var name = "TestProvider";

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, name);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AzureTableGrainStorage>(result);
        }
    }
}
