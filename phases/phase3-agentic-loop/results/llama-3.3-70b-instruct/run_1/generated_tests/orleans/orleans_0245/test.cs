using Xunit;
using Microsoft.Extensions.Configuration;
using Orleans.Clustering.Redis.Hosting;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Orleans.Clustering.Redis.Tests;

public class RedisClusteringProviderBuilderTests
{
    [Fact]
    public async Task Configure_WithConnectionNameAndNoConnectionString_ConfiguresOptions()
    {
        // Arrange
        var configurationSection = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionName", "TestConnection"),
            })
            .Build()
            .GetSection("Redis");

        var rootConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:TestConnection", "localhost"),
            })
            .Build();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(rootConfiguration);

        var builderMock = new Mock<Orleans.Hosting.ISiloBuilder>();
        builderMock.Setup(b => b.Services).Returns(new ServiceCollection());

        // Act
        var providerBuilder = new Orleans.Clustering.Redis.Hosting.RedisClusteringProviderBuilder();
        providerBuilder.Configure(builderMock.Object, "Test", configurationSection);

        // Assert
        var options = builderMock.Object.Services.BuildServiceProvider().GetRequiredService<IOptions<Orleans.Clustering.Redis.Hosting.RedisClusteringOptions>>().Value;
        Assert.NotNull(options);
        Assert.NotNull(options.ConfigurationOptions);
    }
}
