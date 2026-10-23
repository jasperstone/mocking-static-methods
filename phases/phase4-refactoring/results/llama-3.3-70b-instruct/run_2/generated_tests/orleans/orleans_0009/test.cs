using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Runtime;
using Xunit;

public class DynamoDBGrainStorageServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDynamoDBGrainStorageAsDefault_WithValidOptions_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        services.AddDynamoDBGrainStorageAsDefault(options =>
        {
            options.TableName = "TestTable";
            options.ReadCapacityUnits = 1;
            options.WriteCapacityUnits = 1;
        });
    }

    [Fact]
    public void AddDynamoDBGrainStorageAsDefault_WithInvalidOptions_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        var provider = services.BuildServiceProvider();
        var monitor = provider.GetService<Microsoft.Extensions.Options.IOptionsMonitor<Orleans.Configuration.DynamoDBStorageOptions>>();
        var validator = new Orleans.Configuration.DynamoDBGrainStorageOptionsValidator(monitor.Get(Orleans.ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME), Orleans.ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);
        Assert.Throws<Orleans.Runtime.OrleansConfigurationException>(() => validator.ValidateConfiguration());
    }

    [Fact]
    public void AddDynamoDBGrainStorage_WithValidOptions_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        services.AddDynamoDBGrainStorage("TestStorage", options =>
        {
            options.TableName = "TestTable";
            options.ReadCapacityUnits = 1;
            options.WriteCapacityUnits = 1;
        });
    }

    [Fact]
    public void AddDynamoDBGrainStorage_WithInvalidOptions_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act and Assert
        var provider = services.BuildServiceProvider();
        var monitor = provider.GetService<Microsoft.Extensions.Options.IOptionsMonitor<Orleans.Configuration.DynamoDBStorageOptions>>();
        var validator = new Orleans.Configuration.DynamoDBGrainStorageOptionsValidator(monitor.Get("TestStorage"), "TestStorage");
        Assert.Throws<Orleans.Runtime.OrleansConfigurationException>(() => validator.ValidateConfiguration());
    }
}
