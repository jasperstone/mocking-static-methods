using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
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
        public void AddAzureTableTransactionalStateStorage_RegistersIConfigurationValidatorTransient()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "test-provider";

            // Act
            services.AddAzureTableTransactionalStateStorage(name);

            // Assert
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IConfigurationValidator));
            Assert.NotNull(descriptor);
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_CanBuildServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "test-provider";
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            mockOptionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns(new AzureTableTransactionalStateOptions());
            services.AddSingleton<IOptionsMonitor<AzureTableTransactionalStateOptions>>(mockOptionsMonitor.Object);

            // Act
            services.AddAzureTableTransactionalStateStorage(name);
            var serviceProvider = services.BuildServiceProvider();

            // Assert - Successfully exercises GetRequiredService call without throwing
            _ = serviceProvider.GetService<IConfigurationValidator>();
            mockOptionsMonitor.Verify(m => m.Get(name), Times.Once);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_CallsConfigureOptions_WhenProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "test-provider";
            bool configureCalled = false;
            Action<OptionsBuilder<AzureTableTransactionalStateOptions>> configure = _ => configureCalled = true;

            // Act
            services.AddAzureTableTransactionalStateStorage(name, configure);

            // Assert
            Assert.True(configureCalled);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_WithNullConfigureOptions_ReturnsSameServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "test-provider";

            // Act
            var result = services.AddAzureTableTransactionalStateStorage(name, null);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_RegistersMultipleExpectedServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "test-provider";

            // Act
            services.AddAzureTableTransactionalStateStorage(name);

            // Assert
            var descriptors = services.ToList();
            Assert.Contains(descriptors, d => d.ServiceType == typeof(IConfigurationValidator));
            Assert.Contains(descriptors, d => d.ServiceType == typeof(ITransactionalStateStorageFactory) && d.Lifetime == ServiceLifetime.Singleton);
            Assert.Contains(descriptors, d => d.ServiceType == typeof(ILifecycleParticipant<ISiloLifecycle>));
        }
    }
}
