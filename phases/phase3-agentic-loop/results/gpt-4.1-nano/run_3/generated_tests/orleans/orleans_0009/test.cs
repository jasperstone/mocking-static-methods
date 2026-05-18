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

            // Build the service provider so that GetRequiredService can be called
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddTransient<IConfigurationValidator>(sp => new DynamoDBGrainStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>().Get("test"), "test"));

            // Assert
            var provider = services.BuildServiceProvider();
            var validator = provider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }
    }
}
