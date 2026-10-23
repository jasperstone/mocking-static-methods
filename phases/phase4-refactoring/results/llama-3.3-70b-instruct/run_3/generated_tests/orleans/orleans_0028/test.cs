using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans;

namespace Orleans.Storage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_AzureTableGrainStorage_ReturnsInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<AzureTableStorageOptions>();
            services.AddSingleton<IOptionsMonitor<AzureTableStorageOptions>>(new OptionsMonitor<AzureTableStorageOptions>(Options.Create(new AzureTableStorageOptions())));
            services.AddSingleton<ClusterOptions>(new ClusterOptions());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var azureTableGrainStorage = AzureTableGrainStorageFactory.Create(serviceProvider, "test");

            // Assert
            Assert.NotNull(azureTableGrainStorage);
        }

        [Fact]
        public void Create_GetRequiredServiceCalled_ReturnsInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<AzureTableStorageOptions>();
            services.AddSingleton<IOptionsMonitor<AzureTableStorageOptions>>(new OptionsMonitor<AzureTableStorageOptions>(Options.Create(new AzureTableStorageOptions())));
            services.AddSingleton<ClusterOptions>(new ClusterOptions());
            var serviceProvider = services.BuildServiceProvider();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(p => p.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>()).Returns(serviceProvider.GetService<IOptionsMonitor<AzureTableStorageOptions>>());

            // Act
            var azureTableGrainStorage = AzureTableGrainStorageFactory.Create(mockServiceProvider.Object, "test");

            // Assert
            Assert.NotNull(azureTableGrainStorage);
        }
    }
}
