using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Storage;
using Xunit;

namespace Orleans.Storage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_CallsGetRequiredServiceAndReturnsInstance()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptions = new ClusterOptions();
            var options = new AzureTableStorageOptions();
            var name = "TestStorage";

            optionsMonitorMock.Setup(o => o.Get(name)).Returns(options);

            // Setup GetRequiredService to return the mocked IOptionsMonitor
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Setup extension method GetRequiredService to call GetService and throw if null
            // We simulate this by setting up the service provider mock to return the optionsMonitorMock

            // Setup GetProviderClusterOptions extension method
            serviceProviderMock
                .Setup(sp => sp.GetProviderClusterOptions(name))
                .Returns(clusterOptions);

            // Act
            var storage = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, name);

            // Assert
            Assert.NotNull(storage);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetProviderClusterOptions(name), Times.Once);
        }
    }

    // Extension methods used in the factory, mocked for test
    internal static class ServiceProviderExtensions
    {
        public static T GetRequiredService<T>(this IServiceProvider provider)
        {
            var service = (T)provider.GetService(typeof(T));
            if (service == null)
                throw new InvalidOperationException($"Service of type {typeof(T)} not found");
            return service;
        }

        public static ClusterOptions GetProviderClusterOptions(this IServiceProvider provider, string name)
        {
            // For test, just return a new ClusterOptions instance
            return new ClusterOptions();
        }
    }
}
