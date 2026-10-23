using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Orleans.Storage;
using Orleans.Runtime;
using Orleans.Configuration;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldRetrieveOptionsAndCreateInstance()
        {
            // Arrange
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var storageOptions = new DynamoDBStorageOptions
            {
                ServiceId = "test-service",
                TableName = "test-table"
            };
            optionsMonitorMock.Setup(o => o.Get(It.IsAny<string>())).Returns(storageOptions);

            // Custom IServiceProvider implementation
            var serviceProvider = new CustomServiceProvider(optionsMonitorMock.Object);

            // Act
            var storage = DynamoDBGrainStorageFactory.Create(serviceProvider, "testName");

            // Assert
            Assert.NotNull(storage);
            optionsMonitorMock.Verify(o => o.Get("testName"), Times.Once);
        }

        private class CustomServiceProvider : IServiceProvider
        {
            private readonly IOptionsMonitor<DynamoDBStorageOptions> optionsMonitor;

            public CustomServiceProvider(IOptionsMonitor<DynamoDBStorageOptions> optionsMonitor)
            {
                this.optionsMonitor = optionsMonitor;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IOptionsMonitor<DynamoDBStorageOptions>))
                {
                    return optionsMonitor;
                }
                return null;
            }
        }
    }
}
