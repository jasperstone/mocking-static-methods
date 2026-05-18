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
        public void AddAzureTableTransactionalStateStorage_RegistersValidatorUsingGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("testName")).Returns(new AzureTableTransactionalStateOptions());
            services.AddSingleton<IOptionsMonitor<AzureTableTransactionalStateOptions>>(mockOptionsMonitor.Object);

            // Act
            services.AddAzureTableTransactionalStateStorage("testName");

            // Assert - verifies the GetRequiredService call was executed during registration
            var provider = services.BuildServiceProvider();
            var validator = provider.GetServices<IConfigurationValidator>();
            Assert.Single(validator);
            mockOptionsMonitor.Verify(m => m.Get("testName"), Times.Once);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_WithConfigureOptions_InvokesConfigure()
        {
            // Arrange
            var services = new ServiceCollection();
            Action<OptionsBuilder<AzureTableTransactionalStateOptions>> configure = b => { };
            bool configureCalled = false;
            configure = b => configureCalled = true;

            // Act
            var result = services.AddAzureTableTransactionalStateStorage("testName", configure);

            // Assert
            Assert.True(configureCalled);
            Assert.Same(services, result);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_WithoutConfigureOptions_Succeeds()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddAzureTableTransactionalStateStorage("testName");

            // Assert
            Assert.Same(services, result);
        }
    }
}
