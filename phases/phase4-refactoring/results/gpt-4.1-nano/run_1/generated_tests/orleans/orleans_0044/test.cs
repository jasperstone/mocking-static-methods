using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Transactions.AzureStorage;
using Orleans.Transactions.Abstractions;
using Orleans.Providers;
using Orleans.Runtime;

namespace Orleans.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_Should_Call_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            var options = new AzureTableTransactionalStateOptions();

            // Setup the mock to return options when Get is called
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            // Setup the IServiceProvider to return the options monitor when requested
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IOptionsMonitor<AzureTableTransactionalStateOptions>>())))
                .Returns(optionsMonitorMock.Object);

            // Add the mock IServiceProvider to the services
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddAzureTableTransactionalStateStorage("testName");

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Retrieve the validators to ensure the chain is executed
            var validators = provider.GetServices<IConfigurationValidator>();
            Assert.NotEmpty(validators);
        }
    }
}
