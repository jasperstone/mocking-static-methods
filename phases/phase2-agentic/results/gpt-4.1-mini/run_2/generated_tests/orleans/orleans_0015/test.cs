using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans;
using Orleans.Clustering.Cosmos;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_SiloBuilder_WithConnectionName_CallsGetConnectionString()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var optionsBuilderMock = new Mock<IOptionsBuilder<CosmosClusteringOptions>>();
            var optionsMock = new Mock<CosmosClusteringOptions>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var rootConfigurationMock = new Mock<IConfiguration>();

            // Setup configurationSection to return a connectionName and no connectionString
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns(string.Empty);

            // Setup configurationSection indexer for option properties to return null or empty
            configurationSectionMock.Setup(c => c[It.IsAny<string>()]).Returns(string.Empty);

            // Setup service provider to return rootConfiguration when asked for IConfiguration
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

            // Setup rootConfiguration to return a connection string for the given connectionName
            rootConfigurationMock.Setup(rc => rc.GetConnectionString("MyConnectionName")).Returns("FakeConnectionString");

            // Setup optionsBuilder.Configure to invoke the configure delegate immediately
            builderMock.Setup(b => b.UseCosmosClustering(It.IsAny<Action<IOptionsBuilder<CosmosClusteringOptions>>>()))
                .Callback<Action<IOptionsBuilder<CosmosClusteringOptions>>>(configure =>
                {
                    var optionsBuilder = new OptionsBuilderStub<CosmosClusteringOptions>();
                    configure(optionsBuilder);
                })
                .Returns(builderMock.Object);

            var providerBuilder = new CosmosClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builderMock.Object, null, configurationSectionMock.Object);

            // Assert
            rootConfigurationMock.Verify(rc => rc.GetConnectionString("MyConnectionName"), Times.Once);
        }

        // Helper stub to simulate IOptionsBuilder<T> for testing
        private class OptionsBuilderStub<TOptions> : IOptionsBuilder<TOptions> where TOptions : class, new()
        {
            public OptionsBuilderStub()
            {
                Options = new TOptions();
            }

            public TOptions Options { get; }

            public IOptionsBuilder<TOptions> Configure<TDep>(Action<TOptions, TDep> configure)
            {
                // For testing, we simulate IServiceProvider with a mock that returns null for all services
                var serviceProviderMock = new Mock<IServiceProvider>();
                configure(Options, serviceProviderMock.Object);
                return this;
            }
        }
    }
}
