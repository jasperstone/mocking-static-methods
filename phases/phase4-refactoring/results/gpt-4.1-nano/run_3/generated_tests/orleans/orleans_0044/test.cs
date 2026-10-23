using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Hosting;
using Orleans.Transactions.AzureStorage;
using Orleans.Transactions.Abstractions;

namespace Orleans.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        private static class ProviderConstants
        {
            public const string DEFAULT_STORAGE_PROVIDER_NAME = "Default";
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            var options = new AzureTableTransactionalStateOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Setup for GetKeyedService to return a dummy factory
            var factoryMock = new Mock<ITransactionalStateStorageFactory>();
            serviceProviderMock.Setup(sp => sp.GetKeyedService<ITransactionalStateStorageFactory>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME))
                .Returns(factoryMock.Object);

            // Add the extension method
            services.AddSingleton(serviceProviderMock.Object);
            services.AddTransient<IOptionsMonitor<AzureTableTransactionalStateOptions>>(sp => optionsMonitorMock.Object);
            services.AddTransient<IServiceProvider>(sp => sp);

            // Act
            services.AddAzureTableTransactionalStateStorage("testName");

            // Assert
            // Verify that GetRequiredService was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>(), Times.Once);
        }
    }
}
