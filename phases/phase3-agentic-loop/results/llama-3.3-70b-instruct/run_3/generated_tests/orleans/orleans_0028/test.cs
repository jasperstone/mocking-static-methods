using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;
using Moq;

namespace Orleans.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_WithValidServiceProvider_ReturnsAzureTableGrainStorageInstance()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<AzureTableStorageOptions>()
                .AddSingleton<IGrainStorageSerializer>(provider => new AzureTableGrainStorageSerializer())
                .BuildServiceProvider();

            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptions = serviceProvider.GetService<IOptions<ClusterOptions>>();

            var name = "TestStorage";

            // Act
            var storage = AzureTableGrainStorageFactory.Create(serviceProvider, name);

            // Assert
            Assert.NotNull(storage);
            Assert.IsType<IGrainStorage>(storage);
        }

        [Fact]
        public void Create_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => AzureTableGrainStorageFactory.Create(null, "TestStorage"));
        }

        [Fact]
        public void Create_WithNullName_ThrowsArgumentNullException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<AzureTableStorageOptions>()
                .AddSingleton<IGrainStorageSerializer>(provider => new AzureTableGrainStorageSerializer())
                .BuildServiceProvider();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => AzureTableGrainStorageFactory.Create(serviceProvider, null));
        }

        [Fact]
        public void Create_WithEmptyName_ThrowsArgumentException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<AzureTableStorageOptions>()
                .AddSingleton<IGrainStorageSerializer>(provider => new AzureTableGrainStorageSerializer())
                .BuildServiceProvider();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => AzureTableGrainStorageFactory.Create(serviceProvider, string.Empty));
        }
    }
}
