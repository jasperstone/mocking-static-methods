using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.AzureStorage;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting
{
    public class AzureTableTransactionServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_CallsGetRequiredServiceWithCorrectType()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Act
            services.AddAzureTableTransactionalStateStorage("TestName");

            // Assert
            serviceProviderMock.Verify(
                sp => sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>(),
                Times.Once);
        }
    }
}
