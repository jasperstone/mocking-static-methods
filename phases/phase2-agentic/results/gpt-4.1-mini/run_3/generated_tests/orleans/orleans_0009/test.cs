using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_RegistersExpectedServices_AndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a dummy IOptionsMonitor<DynamoDBStorageOptions> to satisfy GetRequiredService call
            var optionsMonitorMock = new OptionsMonitorMock();
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(optionsMonitorMock);

            // Act
            var result = services.AddDynamoDBGrainStorage("TestStorage", ob => { });

            // Assert
            Assert.Same(services, result);

            // Build service provider to test the transient registration
            var provider = services.BuildServiceProvider();

            // Resolve IConfigurationValidator to trigger the factory and thus the GetRequiredService call
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<DynamoDBGrainStorageOptionsValidator>(validator);

            // The validator should have the expected name
            var typedValidator = (DynamoDBGrainStorageOptionsValidator)validator;
            Assert.Equal("TestStorage", typedValidator.Name);
        }

        // A minimal mock of IOptionsMonitor<DynamoDBStorageOptions> to support Get(name)
        private class OptionsMonitorMock : IOptionsMonitor<DynamoDBStorageOptions>
        {
            public DynamoDBStorageOptions CurrentValue => new DynamoDBStorageOptions();

            public DynamoDBStorageOptions Get(string name)
            {
                return new DynamoDBStorageOptions();
            }

            public IDisposable OnChange(Action<DynamoDBStorageOptions, string> listener)
            {
                return null;
            }
        }
    }
}
