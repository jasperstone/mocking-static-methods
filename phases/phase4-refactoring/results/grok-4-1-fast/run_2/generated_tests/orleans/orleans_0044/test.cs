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
        public void AddAzureTableTransactionalStateStorage_RegistersConfigurationValidatorUsingGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("test")).Returns(new AzureTableTransactionalStateOptions());
            services.AddSingleton<IOptionsMonitor<AzureTableTransactionalStateOptions>>(mockOptionsMonitor.Object);

            // Act
            var result = services.AddAzureTableTransactionalStateStorage("test");

            // Assert
            Assert.Same(services, result);
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(validator);
            mockOptionsMonitor.Verify(m => m.Get("test"), Times.Once);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_WithConfigureOptions_CallsConfigureDelegate()
        {
            // Arrange
            var services = new ServiceCollection();
            OptionsBuilder<AzureTableTransactionalStateOptions> capturedBuilder = null;

            // Act
            services.AddAzureTableTransactionalStateStorage("test", builder => 
            {
                capturedBuilder = builder;
            });

            // Assert
            Assert.NotNull(capturedBuilder);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_WithoutConfigureOptions_ReturnsSameServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddAzureTableTransactionalStateStorage("test");

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_RegistersKeyedTransactionalStateStorageFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAzureTableTransactionalStateStorage("test");

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var keyedFactory = serviceProvider.GetKeyedService<ITransactionalStateStorageFactory>("test");
            Assert.NotNull(keyedFactory);
        }
    }
}
