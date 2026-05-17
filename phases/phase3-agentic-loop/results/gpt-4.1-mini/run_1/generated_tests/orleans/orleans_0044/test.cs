using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Transactions.AzureStorage;
using Xunit;

namespace Orleans.Hosting
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_InvokesGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            var options = new AzureTableTransactionalStateOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            // Register the mocked IOptionsMonitor in the service collection
            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            // Call the internal extension method directly since we are in the same namespace
            services.AddAzureTableTransactionalStateStorage("testname", null);

            // Build the service provider to trigger the factory
            var sp = services.BuildServiceProvider();

            // Resolve IConfigurationValidator to trigger the factory and thus the GetRequiredService call
            var validator = sp.GetService<IConfigurationValidator>();

            // Assert
            optionsMonitorMock.Verify(m => m.Get("testname"), Times.AtLeastOnce());
            Assert.NotNull(validator);
        }
    }
}
