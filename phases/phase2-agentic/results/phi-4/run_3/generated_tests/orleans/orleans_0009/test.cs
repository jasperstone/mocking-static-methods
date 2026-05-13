using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_RegistersConfigurationValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new DynamoDBStorageOptions { TableName = "TestTable" });

            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(optionsMonitorMock.Object);

            // Act
            services.AddDynamoDBGrainStorage("TestName");

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(validator);

            var optionsValidator = validator as DynamoDBGrainStorageOptionsValidator;
            Assert.NotNull(optionsValidator);
            Assert.Equal("TestTable", optionsValidator._options.TableName);
        }
    }
}
