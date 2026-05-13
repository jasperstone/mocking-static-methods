using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Storage;
using Moq;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_Should_Call_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a dummy IOptionsMonitor<DynamoDBStorageOptions> to the service collection
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            services.AddSingleton(optionsMonitorMock.Object);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create a new service collection for the test
            var testServices = new ServiceCollection();

            // Add a dummy IOptionsMonitor<DynamoDBStorageOptions> to the test services
            testServices.AddSingleton(optionsMonitorMock.Object);

            // Act
            testServices.AddDynamoDBGrainStorage(
                "TestStorage",
                options => { /* no-op */ }
            );

            // Build the provider
            var provider = testServices.BuildServiceProvider();

            // Retrieve the service to trigger the code
            var service = provider.GetService<IServiceCollection>();

            // Since the extension method adds transient services, we need to resolve the specific validator
            var validator = provider.GetService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            var validatorType = validator.GetType();
            Assert.Equal(typeof(DynamoDBGrainStorageOptionsValidator), validatorType);
        }
    }
}
