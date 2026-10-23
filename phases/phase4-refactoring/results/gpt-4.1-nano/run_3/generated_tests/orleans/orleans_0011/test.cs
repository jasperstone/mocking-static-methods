using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Runtime.MembershipService;
using Orleans.Clustering.AzureStorage;

namespace Orleans.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_ConfiguresServicesAndResolvesValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(configure => configure(services))
                .Returns(builderMock.Object);

            // Act
            // Explicitly specify the overload to avoid ambiguity
            builderMock.Object.UseAzureStorageClustering(opts => { /* no-op */ });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Assert IMembershipTable registration
            var membershipTable = serviceProvider.GetService<IMembershipTable>();
            Assert.NotNull(membershipTable);
            Assert.IsAssignableFrom<IMembershipTable>(membershipTable);

            // Assert validator registration
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AzureStorageClusteringOptionsValidator>(validator);
        }
    }
}
