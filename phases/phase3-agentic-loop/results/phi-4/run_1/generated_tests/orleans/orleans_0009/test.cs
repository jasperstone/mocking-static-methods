using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

// Assuming the namespace for DynamoDBStorageOptions is as follows
using Orleans.Persistence.DynamoDB.Hosting;

namespace Orleans.Persistence.DynamoDB.Hosting.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_ShouldRegisterDynamoDBGrainStorageOptionsValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Act
            services.AddDynamoDBGrainStorage("TestName");

            // Assert
            var provider = services.BuildServiceProvider();
            var validator = provider.GetRequiredService<IConfigurationValidator>();

            Assert.NotNull(validator);
            optionsMonitorMock.Verify(m => m.Get("TestName"), Times.Once);
        }
    }
}
