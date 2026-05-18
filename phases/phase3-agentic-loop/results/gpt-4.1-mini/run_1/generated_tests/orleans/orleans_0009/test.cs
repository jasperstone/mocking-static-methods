using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Xunit;
using Moq;

namespace Orleans.Persistence.DynamoDB.Tests.Hosting
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_WithOptionsBuilder_RegistersExpectedServices_AndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // We need to register IOptionsMonitor<DynamoDBStorageOptions> to avoid failure in the factory delegate
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var options = new DynamoDBStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            var result = services.AddDynamoDBGrainStorage("TestStorage", ob => { });

            // Assert
            Assert.Same(services, result);

            // Build service provider to test the factory delegate
            var provider = services.BuildServiceProvider();

            // Resolve IConfigurationValidator to trigger the factory delegate and thus the GetRequiredService call
            var validator = provider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<DynamoDBGrainStorageOptionsValidator>(validator);
        }
    }
}
