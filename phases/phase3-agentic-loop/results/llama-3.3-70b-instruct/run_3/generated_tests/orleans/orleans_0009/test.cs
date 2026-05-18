using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using Microsoft.Extensions.Options;

namespace Orleans.Tests
{
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
                options.ReadCapacityUnits = 1;
                options.WriteCapacityUnits = 1;
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<Orleans.Configuration.DynamoDBStorageOptions>>();
            var options = optionsMonitor.Get("Default");
            Assert.NotNull(options);
            Assert.Equal("TestTable", options.TableName);
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
                options.ReadCapacityUnits = 1;
                options.WriteCapacityUnits = 1;
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<Orleans.Configuration.DynamoDBStorageOptions>>();
            var options = optionsMonitor.Get("TestStorage");
            Assert.NotNull(options);
            Assert.Equal("TestTable", options.TableName);
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
                });
            });
        }
    }
}
