using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_RegistersDynamoDBGrainStorageOptionsValidator_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            
            // Act
            var result = services.AddDynamoDBGrainStorage("testProvider");

            // Assert
            Assert.Same(services, result);
            var serviceProvider = services.BuildServiceProvider();
            
            // The GetRequiredService call happens during validator resolution, which throws if IOptionsMonitor missing
            // but we verify registration by successful resolution after adding required dependencies
            var validators = serviceProvider.GetServices<IConfigurationValidator>();
            Assert.Single(validators);
            var validator = Assert.IsType<DynamoDBGrainStorageOptionsValidator>(validators.Single());
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_RegistersDynamoDBGrainStorageOptionsValidator_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            
            // Act
            var result = services.AddDynamoDBGrainStorageAsDefault();

            // Assert
            Assert.Same(services, result);
            var serviceProvider = services.BuildServiceProvider();
            var validators = serviceProvider.GetServices<IConfigurationValidator>();
            Assert.Single(validators);
            Assert.IsType<DynamoDBGrainStorageOptionsValidator>(validators.Single());
        }

        [Fact]
        public void AddDynamoDBGrainStorage_WithConfigureOptions_AppliesConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();

            // Act
            services.AddDynamoDBGrainStorage("testProvider", ob => 
                ob.Configure(opts => opts.TableName = "TestTable"));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>().Get("testProvider");
            Assert.Equal("TestTable", options.TableName);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_NullConfigureOptions_RegistersValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            
            // Act
            var result = services.AddDynamoDBGrainStorage("testProvider", (Action<OptionsBuilder<DynamoDBStorageOptions>>)null);

            // Assert
            Assert.Same(services, result);
            var serviceProvider = services.BuildServiceProvider();
            var validator = Assert.IsType<DynamoDBGrainStorageOptionsValidator>(
                serviceProvider.GetServices<IConfigurationValidator>().Single());
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_WithOptionsBuilder_AppliesConfiguration()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();

            // Act
            services.AddDynamoDBGrainStorageAsDefault(ob => 
                ob.Configure(opts => opts.TableName = "DefaultTestTable"));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>().Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);
            Assert.Equal("DefaultTestTable", options.TableName);
        }
    }
}
