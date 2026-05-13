using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Transactions.AzureStorage;
using Orleans.Hosting;
using Xunit;
using Moq;

namespace Orleans.Transactions.AzureStorage.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // We will track if GetRequiredService was called by mocking IServiceProvider
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            var options = new AzureTableTransactionalStateOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableTransactionalStateOptions>)))
                .Returns(optionsMonitorMock.Object);

            // We will add a factory to simulate the call to GetRequiredService inside the AddTransient registration
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddAzureTableTransactionalStateStorage("testName");

            // Build the provider to trigger the registrations
            var provider = services.BuildServiceProvider();

            // Resolve IConfigurationValidator to trigger the factory delegate and thus the GetRequiredService call
            var validator = provider.GetService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            // Verify that GetRequiredService was called on the IServiceProvider mock
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableTransactionalStateOptions>)), Times.AtLeastOnce);
        }
    }
}
