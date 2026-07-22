using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Clustering.Cosmos.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_SiloBuilder_UsesGetConnectionString_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var configurationSection = new TestConfigurationSection();
            configurationSection["ConnectionName"] = "MyConnectionName";
            configurationSection["ConnectionString"] = string.Empty;
            configurationSection["ServiceKey"] = string.Empty;

            var fakeConfiguration = new FakeConfiguration();
            var serviceProvider = new TestServiceProvider(fakeConfiguration);

            // Act
            var connectionName = configurationSection["ConnectionName"];
            var connectionString = configurationSection["ConnectionString"];
            if (!string.IsNullOrEmpty(connectionName) && string.IsNullOrEmpty(connectionString))
            {
                var rootConfiguration = (IConfiguration)serviceProvider.GetService(typeof(IConfiguration));
                connectionString = rootConfiguration.GetConnectionString(connectionName);
            }

            // Assert
            Assert.Equal("FakeConnectionString", connectionString);
            Assert.Equal("MyConnectionName", fakeConfiguration.LastConnectionStringName);
        }

        private class TestConfigurationSection : IConfigurationSection
        {
            private readonly System.Collections.Generic.Dictionary<string, string> _data = new();

            public string this[string key]
            {
                get => _data.TryGetValue(key, out var value) ? value : null;
                set => _data[key] = value;
            }

            public string Key => throw new NotImplementedException();
            public string Path => throw new NotImplementedException();
            public string Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public System.Collections.Generic.IEnumerable<IConfigurationSection> GetChildren() => throw new NotImplementedException();
            public IChangeToken GetReloadToken() => throw new NotImplementedException();
            public IConfigurationSection GetSection(string key) => throw new NotImplementedException();
        }

        private class FakeConfiguration : IConfiguration
        {
            public string LastConnectionStringName { get; private set; }

            public string this[string key]
            {
                get => null;
                set => throw new NotImplementedException();
            }

            public System.Collections.Generic.IEnumerable<IConfigurationSection> GetChildren() => throw new NotImplementedException();
            public IChangeToken GetReloadToken() => throw new NotImplementedException();
            public IConfigurationSection GetSection(string key) => throw new NotImplementedException();

            public string GetConnectionString(string name)
            {
                LastConnectionStringName = name;
                return "FakeConnectionString";
            }
        }

        private class TestServiceProvider : IServiceProvider
        {
            private readonly IConfiguration _configuration;

            public TestServiceProvider(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IConfiguration))
                    return _configuration;
                return null;
            }
        }
    }
}
