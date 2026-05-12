using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Clustering.AdoNet.Tests
{
    public class AdoNetClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_SiloBuilder_UsesConnectionStringFromConfigurationSection()
        {
            // Arrange
            var builderMock = new Mock<ISiloBuilder>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();

            // Setup configurationSection to return null for ConnectionString and a connectionName
            configurationSectionMock.Setup(x => x[nameof(AdoNetClusteringSiloOptions.ConnectionString)]).Returns((string)null);
            configurationSectionMock.Setup(x => x["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(x => x[nameof(AdoNetClusteringSiloOptions.Invariant)]).Returns("InvariantValue");

            // Setup serviceProvider to return IConfiguration
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IConfiguration))).Returns(configurationMock.Object);

            // Setup configuration to return a connection string for the connection name
            configurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("MyConnectionString");

            OptionsBuilder<AdoNetClusteringSiloOptions> capturedOptionsBuilder = null;

            builderMock.Setup(b => b.UseAdoNetClustering(It.IsAny<Action<OptionsBuilder<AdoNetClusteringSiloOptions>>>()))
                .Callback<Action<OptionsBuilder<AdoNetClusteringSiloOptions>>>(configure =>
                {
                    var optionsBuilder = new OptionsBuilder<AdoNetClusteringSiloOptions>(new ServiceCollection());
                    configure(optionsBuilder);
                    capturedOptionsBuilder = optionsBuilder;
                })
                .Returns(builderMock.Object);

            // Act
            var providerBuilder = new AdoNetClusteringProviderBuilder();
            providerBuilder.Configure(builderMock.Object, "name", configurationSectionMock.Object);

            // Assert
            Assert.NotNull(capturedOptionsBuilder);
            var options = new AdoNetClusteringSiloOptions();
            capturedOptionsBuilder.OptionsAction?.Invoke(options, serviceProviderMock.Object);

            Assert.Equal("InvariantValue", options.Invariant);
            Assert.Equal("MyConnectionString", options.ConnectionString);
        }

        [Fact]
        public void Configure_ClientBuilder_UsesConnectionStringFromConfigurationSection()
        {
            // Arrange
            var builderMock = new Mock<IClientBuilder>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();

            // Setup configurationSection to return null for ConnectionString and a connectionName
            configurationSectionMock.Setup(x => x[nameof(AdoNetClusteringClientOptions.ConnectionString)]).Returns((string)null);
            configurationSectionMock.Setup(x => x["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(x => x[nameof(AdoNetClusteringClientOptions.Invariant)]).Returns("InvariantValue");

            // Setup serviceProvider to return IConfiguration
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IConfiguration))).Returns(configurationMock.Object);

            // Setup configuration to return a connection string for the connection name
            configurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("MyConnectionString");

            OptionsBuilder<AdoNetClusteringClientOptions> capturedOptionsBuilder = null;

            builderMock.Setup(b => b.UseAdoNetClustering(It.IsAny<Action<OptionsBuilder<AdoNetClusteringClientOptions>>>()))
                .Callback<Action<OptionsBuilder<AdoNetClusteringClientOptions>>>(configure =>
                {
                    var optionsBuilder = new OptionsBuilder<AdoNetClusteringClientOptions>(new ServiceCollection());
                    configure(optionsBuilder);
                    capturedOptionsBuilder = optionsBuilder;
                })
                .Returns(builderMock.Object);

            // Act
            var providerBuilder = new AdoNetClusteringProviderBuilder();
            providerBuilder.Configure(builderMock.Object, "name", configurationSectionMock.Object);

            // Assert
            Assert.NotNull(capturedOptionsBuilder);
            var options = new AdoNetClusteringClientOptions();
            capturedOptionsBuilder.OptionsAction?.Invoke(options, serviceProviderMock.Object);

            Assert.Equal("InvariantValue", options.Invariant);
            Assert.Equal("MyConnectionString", options.ConnectionString);
        }
    }
}
