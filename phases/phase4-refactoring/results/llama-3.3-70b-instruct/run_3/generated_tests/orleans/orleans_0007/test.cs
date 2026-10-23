using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_ValidOptions_ReturnsServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAdoNetGrainStorage("test", options =>
            {
                options.ConnectionString = "test";
            });

            // Assert
            Assert.NotNull(services);
        }

        [Fact]
        public void AddAdoNetGrainStorage_InvalidOptions_DoesNotThrowException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            services.AddAdoNetGrainStorage("test", options =>
            {
                options.ConnectionString = null;
            });
            Assert.NotNull(services);
        }
    }
}
