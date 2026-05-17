using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Storage;
using System;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_Should_Register_Validator_With_Correct_ServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddDynamoDBGrainStorage("TestName", options => { /* no-op */ });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Act: resolve the validator
            var validator = serviceProvider.GetService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            Assert.IsType<DynamoDBGrainStorageOptionsValidator>(validator);
        }
    }
}
