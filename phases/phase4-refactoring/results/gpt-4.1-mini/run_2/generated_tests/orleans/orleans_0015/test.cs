using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_SiloBuilder_UsesConnectionStringFromRootConfiguration_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var rootConfigurationMock = new Mock<IConfiguration>();

            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);

            rootConfigurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("FakeConnectionString");

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

            var builder = new TestSiloBuilder();

            var providerBuilder = new CosmosClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builder, null, configurationSectionMock.Object);

            // Assert
            Assert.Equal("FakeConnectionString", builder.UsedConnectionString);
        }

        [Fact]
        public void Configure_ClientBuilder_UsesConnectionStringFromRootConfiguration_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var rootConfigurationMock = new Mock<IConfiguration>();

            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);

            rootConfigurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("FakeConnectionString");

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

            var builder = new TestClientBuilder();

            var providerBuilder = new CosmosClusteringProviderBuilder();

            // Act
            providerBuilder.Configure(builder, null, configurationSectionMock.Object);

            // Assert
            Assert.Equal("FakeConnectionString", builder.UsedConnectionString);
        }

        // Minimal test implementation of ISiloBuilder to capture ConfigureCosmosClient call
        private class TestSiloBuilder : ISiloBuilder
        {
            public string? UsedConnectionString { get; set; }

            public IServiceCollection Services => new ServiceCollection();

            public IConfiguration Configuration => new ConfigurationBuilder().Build();

            public ISiloBuilder UseCosmosClustering(Action<ICosmosClusteringOptionsBuilder> configureOptions)
            {
                var optionsBuilder = new TestCosmosClusteringOptionsBuilder(this);
                configureOptions(optionsBuilder);
                return this;
            }
        }

        private class TestCosmosClusteringOptionsBuilder : ICosmosClusteringOptionsBuilder
        {
            private readonly TestSiloBuilder _builder;

            public TestCosmosClusteringOptionsBuilder(TestSiloBuilder builder)
            {
                _builder = builder;
            }

            public ICosmosClusteringOptionsBuilder Configure<TDependency>(Action<ICosmosClusteringOptions, TDependency> configure)
            {
                var options = new TestCosmosClusteringOptions(_builder);
                var services = (TDependency)(object)new ServiceCollection().BuildServiceProvider();
                configure(options, services);
                return this;
            }
        }

        private class TestCosmosClusteringOptions : ICosmosClusteringOptions
        {
            private readonly TestSiloBuilder _builder;

            public TestCosmosClusteringOptions(TestSiloBuilder builder)
            {
                _builder = builder;
            }

            public string DatabaseName { get; set; } = string.Empty;
            public string ContainerName { get; set; } = string.Empty;
            public bool IsResourceCreationEnabled { get; set; }
            public int DatabaseThroughput { get; set; }
            public bool CleanResourcesOnInitialization { get; set; }

            public void ConfigureCosmosClient(Func<IServiceProvider, ValueTask<object>> factory)
            {
                // Not needed for this test
            }

            public void ConfigureCosmosClient(string connectionString)
            {
                _builder.UsedConnectionString = connectionString;
            }
        }

        // Minimal test implementation of IClientBuilder to capture ConfigureCosmosClient call
        private class TestClientBuilder : IClientBuilder
        {
            public string? UsedConnectionString { get; set; }

            public IServiceCollection Services => new ServiceCollection();

            public IConfiguration Configuration => new ConfigurationBuilder().Build();

            public IClientBuilder UseCosmosGatewayListProvider(Action<ICosmosGatewayListProviderOptionsBuilder> configureOptions)
            {
                var optionsBuilder = new TestCosmosGatewayListProviderOptionsBuilder(this);
                configureOptions(optionsBuilder);
                return this;
            }
        }

        private class TestCosmosGatewayListProviderOptionsBuilder : ICosmosGatewayListProviderOptionsBuilder
        {
            private readonly TestClientBuilder _builder;

            public TestCosmosGatewayListProviderOptionsBuilder(TestClientBuilder builder)
            {
                _builder = builder;
            }

            public ICosmosGatewayListProviderOptionsBuilder Configure<TDependency>(Action<ICosmosGatewayListProviderOptions, TDependency> configure)
            {
                var options = new TestCosmosGatewayListProviderOptions(_builder);
                var services = (TDependency)(object)new ServiceCollection().BuildServiceProvider();
                configure(options, services);
                return this;
            }
        }

        private class TestCosmosGatewayListProviderOptions : ICosmosGatewayListProviderOptions
        {
            private readonly TestClientBuilder _builder;

            public TestCosmosGatewayListProviderOptions(TestClientBuilder builder)
            {
                _builder = builder;
            }

            public string DatabaseName { get; set; } = string.Empty;
            public string ContainerName { get; set; } = string.Empty;
            public bool IsResourceCreationEnabled { get; set; }
            public int DatabaseThroughput { get; set; }
            public bool CleanResourcesOnInitialization { get; set; }

            public void ConfigureCosmosClient(Func<IServiceProvider, ValueTask<object>> factory)
            {
                // Not needed for this test
            }

            public void ConfigureCosmosClient(string connectionString)
            {
                _builder.UsedConnectionString = connectionString;
            }
        }
    }

    // Interfaces to support the test
    public interface ICosmosClusteringOptionsBuilder
    {
        ICosmosClusteringOptionsBuilder Configure<TDependency>(Action<ICosmosClusteringOptions, TDependency> configure);
    }

    public interface ICosmosClusteringOptions
    {
        string DatabaseName { get; set; }
        string ContainerName { get; set; }
        bool IsResourceCreationEnabled { get; set; }
        int DatabaseThroughput { get; set; }
        bool CleanResourcesOnInitialization { get; set; }
        void ConfigureCosmosClient(Func<IServiceProvider, ValueTask<object>> factory);
        void ConfigureCosmosClient(string connectionString);
    }

    public interface ICosmosGatewayListProviderOptionsBuilder
    {
        ICosmosGatewayListProviderOptionsBuilder Configure<TDependency>(Action<ICosmosGatewayListProviderOptions, TDependency> configure);
    }

    public interface ICosmosGatewayListProviderOptions
    {
        string DatabaseName { get; set; }
        string ContainerName { get; set; }
        bool IsResourceCreationEnabled { get; set; }
        int DatabaseThroughput { get; set; }
        bool CleanResourcesOnInitialization { get; set; }
        void ConfigureCosmosClient(Func<IServiceProvider, ValueTask<object>> factory);
        void ConfigureCosmosClient(string connectionString);
    }
}
