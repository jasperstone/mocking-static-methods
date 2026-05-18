using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using Moq;
using Xunit;

namespace Orleans.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_ValidOptions_AddsGrainStorage()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            options.Setup(o => o.Get("test")).Returns(new AdoNetGrainStorageOptions());

            // Act
            services.AddAdoNetGrainStorage("test", ob => ob.Configure(o => { }));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainStorage = serviceProvider.GetService<IGrainStorage>();
            Assert.NotNull(grainStorage);
        }

        [Fact]
        public void AddAdoNetGrainStorage_InvalidOptions_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            options.Setup(o => o.Get("test")).Returns(new AdoNetGrainStorageOptions { Invariant = "Invalid" });

            // Act and Assert
            Assert.Throws<Exception>(() => services.AddAdoNetGrainStorage("test", ob => ob.Configure(o => { })));
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_ValidOptions_AddsGrainStorage()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            options.Setup(o => o.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)).Returns(new AdoNetGrainStorageOptions());

            // Act
            services.AddAdoNetGrainStorageAsDefault(ob => ob.Configure(o => { }));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainStorage = serviceProvider.GetService<IGrainStorage>();
            Assert.NotNull(grainStorage);
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_InvalidOptions_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            options.Setup(o => o.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)).Returns(new AdoNetGrainStorageOptions { Invariant = "Invalid" });

            // Act and Assert
            Assert.Throws<Exception>(() => services.AddAdoNetGrainStorageAsDefault(ob => ob.Configure(o => { })));
        }
    }

    public class AdoNetGrainStorageOptions
    {
        public string Invariant { get; set; }
    }

    public static class ProviderConstants
    {
        public const string DEFAULT_STORAGE_PROVIDER_NAME = "Default";
    }
}
