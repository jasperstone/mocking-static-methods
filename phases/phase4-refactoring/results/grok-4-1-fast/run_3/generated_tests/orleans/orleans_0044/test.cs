using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
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
        public void AddAzureTableTransactionalStateStorage_RegistersIConfigurationValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("testName")).Returns(new AzureTableTransactionalStateOptions());
            services.AddSingleton<IOptionsMonitor<AzureTableTransactionalStateOptions>>(mockOptionsMonitor.Object);

            // Act
            var resultServices = AzureTableTransactionServicecollectionExtensions.AddAzureTableTransactionalStateStorage(services, "testName");

            // Assert
            Assert.Same(services, resultServices);
            var serviceProvider = resultServices.BuildServiceProvider();
            var validators = serviceProvider.GetServices<IConfigurationValidator>();
            Assert.Single(validators);
            mockOptionsMonitor.Verify(m => m.Get("testName"), Times.Once);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_CallsConfigureOptions_WhenProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            bool configureCalled = false;
            Action<OptionsBuilder<AzureTableTransactionalStateOptions>> configure = builder =>
            {
                configureCalled = true;
            };

            // Act
            AzureTableTransactionServicecollectionExtensions.AddAzureTableTransactionalStateStorage(services, "testName", configure);

            // Assert
            Assert.True(configureCalled);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_ReturnsSameServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = AzureTableTransactionServicecollectionExtensions.AddAzureTableTransactionalStateStorage(services, "testName");

            // Assert
            Assert.Same(services, result);
        }
    }
}
