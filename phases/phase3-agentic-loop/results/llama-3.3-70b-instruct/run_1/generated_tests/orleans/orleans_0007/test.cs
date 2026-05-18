using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Persistence.AdoNet;
using Xunit;

namespace Orleans.Persistence.AdoNet.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_ValidOptions_ServiceProviderCreated()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestStorage";

            // Act
            services.AddAdoNetGrainStorage(name);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var adoNetGrainStorageOptionsValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(adoNetGrainStorageOptionsValidator);
        }

        [Fact]
        public void AddAdoNetGrainStorage_InvalidOptions_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestStorage";

            // Act and Assert
            Assert.Throws<Exception>(() => services.AddAdoNetGrainStorage(name));
        }
    }
}
