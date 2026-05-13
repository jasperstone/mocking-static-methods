using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_ValidOptions_ServiceProviderCreated()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DynamoDBStorageOptions
            {
                TableName = "TestTable",
                ReadCapacityUnits = 1,
                WriteCapacityUnits = 1,
            };

            // Act
            services.AddDynamoDBGrainStorage("TestStorage", ob => ob.Configure(options));
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            var monitor = serviceProvider.GetService<IOptionsMonitor<DynamoDBStorageOptions>>();
            Assert.NotNull(monitor);
            var optionsValue = monitor.Get("TestStorage");
            Assert.NotNull(optionsValue);
            Assert.Equal("TestTable", optionsValue.TableName);
            Assert.Equal(1, optionsValue.ReadCapacityUnits);
            Assert.Equal(1, optionsValue.WriteCapacityUnits);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_InvalidOptions_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DynamoDBStorageOptions
            {
                TableName = string.Empty,
                ReadCapacityUnits = 0,
                WriteCapacityUnits = 0,
            };

            // Act and Assert
            services.AddDynamoDBGrainStorage("TestStorage", ob => ob.Configure(options));
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.Throws<OrleansConfigurationException>(() => validator.ValidateConfiguration());
        }
    }
}
