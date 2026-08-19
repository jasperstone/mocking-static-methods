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
    // Minimal stub for AzureTableTransactionalStateOptions
    public class AzureTableTransactionalStateOptions { }

    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_Should_Call_GetRequiredService_ForOptionsMonitor()
        {
            // Arrange
            var services = new ServiceCollection();

            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            var options = new AzureTableTransactionalStateOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Register the mock service provider
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            // Call the extension method
            services.AddAzureTableTransactionalStateStorage("testName");

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Assert
            // Verify that the validator was created with the options returned by the mock
            var validators = provider.GetServices<IConfigurationValidator>();
            Assert.Contains(validators, v => v.GetType().Name.Contains("AzureTableTransactionalStateOptionsValidator"));
        }
    }
}
