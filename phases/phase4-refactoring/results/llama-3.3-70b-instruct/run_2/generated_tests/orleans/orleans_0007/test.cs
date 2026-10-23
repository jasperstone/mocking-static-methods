using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Persistence.AdoNet;
using Moq;

namespace Orleans.Persistence.AdoNet.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_ValidOptions_AddsAdoNetGrainStorage()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAdoNetGrainStorage("test", ob => ob.Configure(options => { }));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var adoNetGrainStorageOptions = serviceProvider.GetService<Microsoft.Extensions.Options.IOptionsMonitor<Orleans.Configuration.AdoNetGrainStorageOptions>>().Get("test");
            Assert.NotNull(adoNetGrainStorageOptions);
        }

        [Fact]
        public void AddAdoNetGrainStorage_InvalidOptions_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<Exception>(() => services.AddAdoNetGrainStorage(null, ob => ob.Configure(options => { })));
        }
    }
}
