using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.AzureStorage;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_CallsGetRequiredServiceWithIOptionsMonitor()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Act
            AzureTableTransactionServicecollectionExtensions.AddAzureTableTransactionalStateStorage(services, "TestName");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>(), Times.Once);
        }
    }
}
