using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using Orleans.Configuration;
using Microsoft.Extensions.Options;

namespace Orleans.Hosting.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_WithConfigureOptions_ConfiguresOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<Orleans.Configuration.DynamoDBStorageOptions>(options =>
            {
                options.TableName = "TestTable";
            });

            // Act
            services.AddDynamoDBGrainStorageAsDefault(configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptionsMonitor<Orleans.Configuration.DynamoDBStorageOptions>>().Get("Memory");
            Assert.Equal("TestTable", options.TableName);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_WithoutConfigureOptions_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            services.AddDynamoDBGrainStorageAsDefault();
        }

        [Fact]
        public void AddDynamoDBGrainStorage_WithConfigureOptions_ConfiguresOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<Orleans.Configuration.DynamoDBStorageOptions>(options =>
            {
                options.TableName = "TestTable";
            });
            var name = "TestName";

            // Act
            services.AddDynamoDBGrainStorage(name, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptionsMonitor<Orleans.Configuration.DynamoDBStorageOptions>>().Get(name);
            Assert.Equal("TestTable", options.TableName);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_WithoutConfigureOptions_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestName";

            // Act and Assert
            services.AddDynamoDBGrainStorage(name);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_WithOptionsBuilder_ConfiguresOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<Microsoft.Extensions.Options.OptionsBuilder<Orleans.Configuration.DynamoDBStorageOptions>>(options =>
            {
                options.Configure(options => options.TableName = "TestTable");
            });

            // Act
            services.AddDynamoDBGrainStorageAsDefault(configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptionsMonitor<Orleans.Configuration.DynamoDBStorageOptions>>().Get("Memory");
            Assert.Equal("TestTable", options.TableName);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_WithOptionsBuilder_ConfiguresOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<Microsoft.Extensions.Options.OptionsBuilder<Orleans.Configuration.DynamoDBStorageOptions>>(options =>
            {
                options.Configure(options => options.TableName = "TestTable");
            });
            var name = "TestName";

            // Act
            services.AddDynamoDBGrainStorage(name, configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptionsMonitor<Orleans.Configuration.DynamoDBStorageOptions>>().Get(name);
            Assert.Equal("TestTable", options.TableName);
        }
    }
}
