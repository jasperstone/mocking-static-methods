using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Transactions.Abstractions;
using Xunit;

namespace Orleans.Transactions.AzureStorage.Tests
{
    public class AzureTableTransactionServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_RegistersIConfigurationValidator()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableTransactionalStateOptions>)))
                .Returns(optionsMonitorMock.Object);

            serviceCollection.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            serviceCollection.AddAzureTableTransactionalStateStorage("TestStorage");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();

            Assert.NotNull(configurationValidator);
        }
    }
}
