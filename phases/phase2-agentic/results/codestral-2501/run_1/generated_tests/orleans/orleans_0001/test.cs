using Xunit;
using Orleans.Hosting;
using Orleans.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;

namespace Orleans.Clustering.AdoNet.Tests
{
    public class AdoNetClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_Silo_ShouldSetConnectionStringFromConfiguration()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(x => x[nameof(AdoNetClusteringSiloOptions.Invariant)]).Returns("InvariantValue");
            mockConfigurationSection.Setup(x => x[nameof(AdoNetClusteringSiloOptions.ConnectionString)]).Returns("ConnectionStringValue");
            mockConfigurationSection.Setup(x => x["ConnectionName"]).Returns("ConnectionNameValue");

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(x => x.GetConnectionString("ConnectionNameValue")).Returns("FetchedConnectionString");

            var services = new ServiceCollection();
            services.AddSingleton(mockConfiguration.Object);
            var serviceProvider = services.BuildServiceProvider();

            var builder = new Mock<ISiloBuilder>();
            var optionsBuilder = new Mock<OptionsBuilder<AdoNetClusteringSiloOptions>>();
            var options = new AdoNetClusteringSiloOptions();

            builder.Setup(x => x.UseAdoNetClustering(It.IsAny<Action<OptionsBuilder<AdoNetClusteringSiloOptions>>>()))
                .Callback<Action<OptionsBuilder<AdoNetClusteringSiloOptions>>>(action => action(optionsBuilder.Object));

            optionsBuilder.Setup(x => x.Configure<IServiceProvider>(It.IsAny<Action<AdoNetClusteringSiloOptions, IServiceProvider>>()))
                .Callback<Action<AdoNetClusteringSiloOptions, IServiceProvider>>((options, services) =>
                {
                    options.Invariant = mockConfigurationSection.Object[nameof(AdoNetClusteringSiloOptions.Invariant)];
                    options.ConnectionString = mockConfigurationSection.Object[nameof(AdoNetClusteringSiloOptions.ConnectionString)];
                    if (string.IsNullOrEmpty(options.ConnectionString) && !string.IsNullOrEmpty(mockConfigurationSection.Object["ConnectionName"]))
                    {
                        options.ConnectionString = services.GetRequiredService<IConfiguration>().GetConnectionString(mockConfigurationSection.Object["ConnectionName"]);
                    }
                });

            var providerBuilder = new AdoNetClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builder.Object, "AdoNet", mockConfigurationSection.Object);

            // Assert
            Assert.Equal("FetchedConnectionString", options.ConnectionString);
        }

        [Fact]
        public void Configure_Client_ShouldSetConnectionStringFromConfiguration()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(x => x[nameof(AdoNetClusteringSiloOptions.Invariant)]).Returns("InvariantValue");
            mockConfigurationSection.Setup(x => x[nameof(AdoNetClusteringSiloOptions.ConnectionString)]).Returns("ConnectionStringValue");
            mockConfigurationSection.Setup(x => x["ConnectionName"]).Returns("ConnectionNameValue");

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(x => x.GetConnectionString("ConnectionNameValue")).Returns("FetchedConnectionString");

            var services = new ServiceCollection();
            services.AddSingleton(mockConfiguration.Object);
            var serviceProvider = services.BuildServiceProvider();

            var builder = new Mock<IClientBuilder>();
            var optionsBuilder = new Mock<OptionsBuilder<AdoNetClusteringClientOptions>>();
            var options = new AdoNetClusteringClientOptions();

            builder.Setup(x => x.UseAdoNetClustering(It.IsAny<Action<OptionsBuilder<AdoNetClusteringClientOptions>>>()))
                .Callback<Action<OptionsBuilder<AdoNetClusteringClientOptions>>>(action => action(optionsBuilder.Object));

            optionsBuilder.Setup(x => x.Configure<IServiceProvider>(It.IsAny<Action<AdoNetClusteringClientOptions, IServiceProvider>>()))
                .Callback<Action<AdoNetClusteringClientOptions, IServiceProvider>>((options, services) =>
                {
                    options.Invariant = mockConfigurationSection.Object[nameof(AdoNetClusteringSiloOptions.Invariant)];
                    options.ConnectionString = mockConfigurationSection.Object[nameof(AdoNetClusteringSiloOptions.ConnectionString)];
                    if (string.IsNullOrEmpty(options.ConnectionString) && !string.IsNullOrEmpty(mockConfigurationSection.Object["ConnectionName"]))
                    {
                        options.ConnectionString = services.GetRequiredService<IConfiguration>().GetConnectionString(mockConfigurationSection.Object["ConnectionName"]);
                    }
                });

            var providerBuilder = new AdoNetClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builder.Object, "AdoNet", mockConfigurationSection.Object);

            // Assert
            Assert.Equal("FetchedConnectionString", options.ConnectionString);
        }
    }
}
