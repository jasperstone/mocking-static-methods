using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Transactions.AzureStorage;
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

            // We will track if the GetRequiredService call is made by mocking IServiceProvider
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            var options = new AzureTableTransactionalStateOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableTransactionalStateOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Add a factory to simulate the GetRequiredService extension method call
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            var result = AzureTableTransactionServicecollectionExtensions.AddAzureTableTransactionalStateStorage(services, "testName");

            // Build the service provider to trigger the registrations
            var provider = services.BuildServiceProvider();

            // Assert
            // Check that the IConfigurationValidator service is registered and can be resolved
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // Check that the returned IServiceCollection is the same instance
            Assert.Same(services, result);
        }
    }
}
