using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using System;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_WithConfigureOptions_ServiceProviderHasRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<DynamoDBStorageOptions>(options =>
            {
                options.TableName = "TestTable";
            });

            // Act
            services.AddDynamoDBGrainStorageAsDefault(configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var requiredService = serviceProvider.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>();
            Assert.NotNull(requiredService);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_WithConfigureOptions_ServiceProviderHasRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestName";
            var configureOptions = new Action<DynamoDBStorageOptions>(options =>
            {
                options.TableName = "TestTable";
            });

            // Act
            services.AddDynamoDBGrainStorage(name, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var requiredService = serviceProvider.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>();
            Assert.NotNull(requiredService);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_WithoutConfigureOptions_ServiceProviderHasRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddDynamoDBGrainStorageAsDefault();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var requiredService = serviceProvider.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>();
            Assert.NotNull(requiredService);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_WithoutConfigureOptions_ServiceProviderHasRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestName";

            // Act
            services.AddDynamoDBGrainStorage(name);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var requiredService = serviceProvider.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>();
            Assert.NotNull(requiredService);
        }
    }
}
