using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Orleans.Clustering.Cosmos.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void GetConnectionString_ShouldRetrieveConnectionStringFromConfiguration()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var mockConfigurationSection = new Mock<IConfigurationSection>();

            string connectionName = "TestConnection";
            string expectedConnectionString = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=12345;";

            mockConfiguration.Setup(c => c.GetConnectionString(connectionName)).Returns(expectedConnectionString);
            mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns(connectionName);
            mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns(string.Empty);

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(mockConfiguration.Object);

            var options = new Mock<CosmosClusteringOptions>();
            var optionsBuilder = new Mock<Action<CosmosClusteringOptions, IServiceProvider>>();

            // Mock the ConfigureCosmosClient method
            options.Setup(o => o.ConfigureCosmosClient(It.IsAny<string>()))
                   .Callback<string>(connString => Assert.Equal(expectedConnectionString, connString));

            // Act
            var builder = new Mock<IProviderBuilder<ISiloBuilder>>();
            builder.Setup(b => b.Configure(It.IsAny<ISiloBuilder>(), null, It.IsAny<IConfigurationSection>()))
                   .Callback<ISiloBuilder, string, IConfigurationSection>((siloBuilder, name, section) =>
                   {
                       var rootConfiguration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                       var connectionString = rootConfiguration.GetConnectionString(connectionName);
                       optionsBuilder.Invoke(options.Object, services.BuildServiceProvider());
                   });

            builder.Object.Configure(new Mock<ISiloBuilder>().Object, null, mockConfigurationSection.Object);

            // Assert
            mockConfiguration.Verify(c => c.GetConnectionString(connectionName), Times.Once);
        }
    }
}
