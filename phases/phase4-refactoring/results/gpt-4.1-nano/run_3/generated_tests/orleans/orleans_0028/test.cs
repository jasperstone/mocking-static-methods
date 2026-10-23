using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Providers.Azure;
using Orleans.Storage;
using System;

namespace Orleans.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldRetrieveOptionsAndCreateStorage()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var storageOptions = new AzureTableStorageOptions();
            optionsMonitorMock.Setup(o => o.Get(It.IsAny<string>())).Returns(storageOptions);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            var serviceProvider = serviceProviderMock.Object;
            string providerName = "TestProvider";

            // Act
            var storage = AzureTableGrainStorageFactory.Create(serviceProvider, providerName);

            // Assert
            Assert.NotNull(storage);
        }
    }
}
