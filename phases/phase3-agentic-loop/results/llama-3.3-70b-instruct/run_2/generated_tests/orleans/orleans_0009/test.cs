using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Runtime;
using Xunit;

public class DynamoDBGrainStorageServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDynamoDBGrainStorageAsDefault_WithValidOptions_ConfiguresServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDynamoDBGrainStorageAsDefault(options =>
        {
            options.TableName = "TestTable";
            options.UseProvisionedThroughput = true;
            options.ReadCapacityUnits = 1;
            options.WriteCapacityUnits = 1;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<Orleans.Persistence.DynamoDB.Options.DynamoDBStorageOptions>>();
        var options = optionsMonitor.Get(Orleans.Configuration.ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);
        Assert.NotNull(options);
        Assert.Equal("TestTable", options.TableName);
        Assert.True(options.UseProvisionedThroughput);
        Assert.Equal(1, options.ReadCapacityUnits);
        Assert.Equal(1, options.WriteCapacityUnits);
    }

    [Fact]
    public void AddDynamoDBGrainStorageAsDefault_WithInvalidOptions_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        Assert.Throws<OrleansConfigurationException>(() =>
        {
            services.AddDynamoDBGrainStorageAsDefault(options =>
            {
                options.TableName = string.Empty;
                options.UseProvisionedThroughput = true;
                options.ReadCapacityUnits = 0;
                options.WriteCapacityUnits = 0;
            });
        });
    }

    [Fact]
    public void AddDynamoDBGrainStorage_WithValidOptions_ConfiguresServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDynamoDBGrainStorage("TestStorage", options =>
        {
            options.TableName = "TestTable";
            options.UseProvisionedThroughput = true;
            options.ReadCapacityUnits = 1;
            options.WriteCapacityUnits = 1;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<Orleans.Persistence.DynamoDB.Options.DynamoDBStorageOptions>>();
        var options = optionsMonitor.Get("TestStorage");
        Assert.NotNull(options);
        Assert.Equal("TestTable", options.TableName);
        Assert.True(options.UseProvisionedThroughput);
        Assert.Equal(1, options.ReadCapacityUnits);
        Assert.Equal(1, options.WriteCapacityUnits);
    }

    [Fact]
    public void AddDynamoDBGrainStorage_WithInvalidOptions_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        Assert.Throws<OrleansConfigurationException>(() =>
        {
            services.AddDynamoDBGrainStorage("TestStorage", options =>
            {
                options.TableName = string.Empty;
                options.UseProvisionedThroughput = true;
                options.ReadCapacityUnits = 0;
                options.WriteCapacityUnits = 0;
            });
        });
    }
}
