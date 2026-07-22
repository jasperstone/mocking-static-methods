using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using Orleans.Clustering.Cosmos;
using System;

public class CosmosClusteringProviderBuilderTests
{
    [Fact]
    public void Configure_CallsGetConnectionString_WhenConnectionNameIsSpecified()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("ConnectionStrings:TestConnection", "TestConnectionString")
            })
            .Build();
        var configurationSection = configuration.GetSection("TestSection");
        configurationSection["ConnectionName"] = "TestConnection";
        var rootConfiguration = new Mock<IConfiguration>();
        rootConfiguration.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");
        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetService(typeof(IConfiguration))).Returns(rootConfiguration.Object);

        // Act
        var builder = new Orleans.Hosting.CosmosClusteringProviderBuilder();
        builder.Configure(new Mock<Orleans.ISiloBuilder>().Object, null, configurationSection);

        // Assert
        rootConfiguration.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
    }
}
