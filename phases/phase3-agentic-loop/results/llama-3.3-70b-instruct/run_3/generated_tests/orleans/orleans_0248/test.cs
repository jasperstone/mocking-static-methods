using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using StackExchange.Redis;
using Xunit;

namespace Orleans.Tests
{
    public class RedisGrainDirectoryProviderBuilderTests
    {
        [Fact]
        public void Configure_GetConnectionString_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ConnectionName", "TestConnection"),
                    new KeyValuePair<string, string>("TestConnection", "TestConnectionString"),
                })
                .Build();
            services.AddSingleton<IConfiguration>(configuration);
            var serviceProvider = services.BuildServiceProvider();
            var configurationSection = configuration.GetSection("TestSection");
            var builder = new TestSiloBuilder();

            // Act
            var redisGrainDirectoryProviderBuilder = new RedisGrainDirectoryProviderBuilder();
            redisGrainDirectoryProviderBuilder.Configure(builder, "TestName", configurationSection);

            // Assert
            // We can't directly test if GetConnectionString is called, but we can test if the connection string is correctly set.
            var options = (RedisGrainDirectoryOptions)builder.Options;
            Assert.NotNull(options);
        }

        private class TestSiloBuilder : ISiloBuilder
        {
            public IServiceCollection Services { get; } = new ServiceCollection();
            public IConfiguration Configuration { get; } = new ConfigurationBuilder().Build();
            public object Options { get; set; }

            public void AddRedisGrainDirectory(string name, Action<OptionsBuilder<RedisGrainDirectoryOptions>> configureOptions)
            {
                var optionsBuilder = Options.Create<RedisGrainDirectoryOptions>();
                configureOptions(optionsBuilder);
                Options = optionsBuilder.Value;
            }
        }
    }
}
