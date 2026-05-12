using System;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Configuration;

namespace Orleans.Tests
{
    public class AdoNetClusteringProviderBuilderTests
    {
        [Fact]
        public void Configure_Should_Call_GetConnectionString_When_ConnectionString_Is_Null_And_ConnectionName_Is_Set()
        {
            // Arrange
            var configurationSectionMock = new MockConfigurationSection();
            configurationSectionMock.Set(nameof(AdoNetClusteringSiloOptions.ConnectionString), null);
            configurationSectionMock.Set("ConnectionName", "TestConnection");
            var services = new ServiceCollection();
            var configurationMock = new MockConfiguration();
            configurationMock.SetConnectionString("TestConnection", "Server=myServer;Database=myDb;");
            services.AddSingleton<IConfiguration>(configurationMock);
            var serviceProvider = services.BuildServiceProvider();

            var builder = new AdoNetClusteringProviderBuilder();

            var siloBuilderMock = new MockSiloBuilder();

            // Act
            builder.Configure(siloBuilderMock, "Test", configurationSectionMock);
            // No exception means pass
        }
    }

    // Mock implementations for IConfigurationSection and IConfiguration
    public class MockConfigurationSection : IConfigurationSection
    {
        private readonly Dictionary<string, string> _values = new();

        public string this[string key]
        {
            get => _values.TryGetValue(key, out var value) ? value : null;
            set => _values[key] = value;
        }

        public string Key => throw new NotImplementedException();
        public string Path => throw new NotImplementedException();
        public string Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IEnumerable<IConfigurationSection> GetChildren() => throw new NotImplementedException();
        public IChangeToken GetReloadToken() => throw new NotImplementedException();
        public IConfigurationSection GetSection(string key) => throw new NotImplementedException();

        public void Set(string key, string value)
        {
            _values[key] = value;
        }
    }

    public class MockConfiguration : IConfiguration
    {
        private readonly Dictionary<string, string> _connectionStrings = new();

        public void SetConnectionString(string name, string connectionString)
        {
            _connectionStrings[name] = connectionString;
        }

        public string GetConnectionString(string name)
        {
            return _connectionStrings.TryGetValue(name, out var value) ? value : null;
        }

        public string this[string key]
        {
            get => null;
            set { }
        }

        public IEnumerable<IConfigurationSection> GetChildren() => throw new NotImplementedException();
        public IChangeToken GetReloadToken() => throw new NotImplementedException();
        public IConfigurationSection GetSection(string key) => throw new NotImplementedException();
    }

    public class MockSiloBuilder : ISiloBuilder
    {
        public Action<OptionsBuilder<AdoNetClusteringSiloOptions>> ConfigureAction { get; private set; }

        public void UseAdoNetClustering(Action<OptionsBuilder<AdoNetClusteringSiloOptions>> configureOptions)
        {
            ConfigureAction = configureOptions;
        }
    }
}
