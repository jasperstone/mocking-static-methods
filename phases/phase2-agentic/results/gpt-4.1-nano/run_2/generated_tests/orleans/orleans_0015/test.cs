using System;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Clustering.Cosmos;
using Moq;
using System.Threading.Tasks;

namespace Orleans.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        private class DummyOptions
        {
            public string DatabaseName { get; set; }
            public string ContainerName { get; set; }
            public bool IsResourceCreationEnabled { get; set; }
            public int DatabaseThroughput { get; set; }
            public bool CleanResourcesOnInitialization { get; set; }
            public Action<Func<IServiceProvider, ValueTask<CosmosClient>>> ConfigureCosmosClient { get; set; }
        }

        [Fact]
        public void Configure_Should_Call_GetConnectionString_When_ServiceKey_IsEmpty_And_ConnectionName_IsSet_And_ConnectionString_IsEmpty()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionName", "MyConnection"},
                {"ConnectionString", ""}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var serviceCollection = new ServiceCollection();
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetConnectionString("MyConnection")).Returns("Server=myServer;Database=myDb;");
            serviceCollection.AddSingleton(mockConfig.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("MyConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns("");

            var options = new DummyOptions();

            var builder = new Mock<IProviderBuilder<IClientBuilder>>();
            var clientBuilder = new Mock<IClientBuilder>();
            var optionsBuilder = new Action<Mock<DummyOptions>>(opts => { });

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(mockConfig.Object);
            var serviceProvider2 = services.BuildServiceProvider();

            var builderInstance = new CosmosClusteringProviderBuilder();

            // Act
            builderInstance.Configure(new Mock<IClientBuilder>().Object, null, configurationSectionMock.Object);

            // Assert
            mockConfig.Verify(c => c.GetConnectionString("MyConnection"), Times.Once);
        }
    }
}
