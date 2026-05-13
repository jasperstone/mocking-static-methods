using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Storage;
using Moq;
using Microsoft.Extensions.Options;
using System;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_ShouldAddDynamoDBGrainStorage()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DynamoDBStorageOptions();

            // Act
            services.AddDynamoDBGrainStorageAsDefault(opts => opts.Configure(o => o = options));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainStorage = serviceProvider.GetService<IGrainStorage>();
            Assert.NotNull(grainStorage);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ShouldAddDynamoDBGrainStorage()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DynamoDBStorageOptions();

            // Act
            services.AddDynamoDBGrainStorage("TestStorage", opts => opts.Configure(o => o = options));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainStorage = serviceProvider.GetService<IGrainStorage>();
            Assert.NotNull(grainStorage);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ShouldThrowIfServiceProviderIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DynamoDBStorageOptions();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => services.AddDynamoDBGrainStorage("TestStorage", opts => opts.Configure(o => o = options)));
        }
    }
}
