using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Persistence.DynamoDB.Hosting; // Assuming this is the correct namespace for the extensions

namespace Orleans.Persistence.DynamoDB.Hosting.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_ShouldRegisterDynamoDBGrainStorageOptionsValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestStorage";
            var optionsMonitorMock = new Mock<IOptionsMonitor<object>>(); // Using object as a placeholder
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<object>>()) // Using object as a placeholder
                .Returns(optionsMonitorMock.Object);

            // Act
            DynamoDBGrainStorageServiceCollectionExtensions.AddDynamoDBGrainStorage(services, name, null);

            // Assert
            var provider = services.BuildServiceProvider();
            var validator = provider.GetRequiredService<IConfigurationValidator>();

            Assert.NotNull(validator);
            optionsMonitorMock.Verify(m => m.Get(It.IsAny<string>()), Times.Once);
        }
    }
}
