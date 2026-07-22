using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Storage;
using Orleans.Configuration;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldRetrieveOptionsAndCreateStorage()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register a mock IOptionsMonitor<DynamoDBStorageOptions>
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var storageOptions = new DynamoDBStorageOptions
            {
                ServiceId = "test-service",
                TableName = "test-table"
            };
            optionsMonitorMock.Setup(o => o.Get(It.IsAny<string>())).Returns(storageOptions);
            services.AddSingleton(optionsMonitorMock.Object);

            // Register a mock IActivatorProvider
            var activatorProviderMock = new Mock<IActivatorProvider>();
            services.AddSingleton<IActivatorProvider>(activatorProviderMock.Object);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            string storageName = "testStorage";

            // Act
            var storage = DynamoDBGrainStorageFactory.Create(serviceProvider, storageName);

            // Assert
            Assert.NotNull(storage);
            Assert.IsType<DynamoDBGrainStorage>(storage);
        }
    }
}
