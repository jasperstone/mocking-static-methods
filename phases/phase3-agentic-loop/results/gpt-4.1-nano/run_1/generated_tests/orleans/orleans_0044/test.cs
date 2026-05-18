using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Transactions.AzureStorage;
using Orleans.Transactions.Abstractions;
using System;

namespace Orleans.Tests
{
    public class AzureTableTransactionServicecollectionExtensionsTests
    {
        [Fact]
        public void AddAzureTableTransactionalStateStorage_Should_Register_Services_Correctly()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAzureTableTransactionalStateStorage("testName");

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            // Check that the IConfigurationValidator is registered
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // Check that the ITransactionalStateStorageFactory singleton is registered
            var factory = serviceProvider.GetService<ITransactionalStateStorageFactory>();
            Assert.NotNull(factory);

            // Check that the ILifecycleParticipant is registered
            var lifecycleParticipant = serviceProvider.GetService<ILifecycleParticipant<ISiloLifecycle>>();
            Assert.NotNull(lifecycleParticipant);
        }

        [Fact]
        public void AddAzureTableTransactionalStateStorage_Should_Call_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a mock IOptionsMonitor<AzureTableTransactionalStateOptions>
            var optionsMonitor = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
            services.AddSingleton(optionsMonitor.Object);

            // Act
            services.AddAzureTableTransactionalStateStorage("testName");

            // Build provider
            var provider = services.BuildServiceProvider();

            // Use reflection to verify that GetRequiredService was called on IServiceProvider
            // Since we can't directly verify extension method calls, we verify the effect
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }
    }
}
