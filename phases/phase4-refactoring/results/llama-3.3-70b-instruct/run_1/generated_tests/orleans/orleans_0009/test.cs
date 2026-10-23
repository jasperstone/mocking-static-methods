using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Persistence.DynamoDB;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_GetRequiredServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var optionsMock = new Mock<IOptions<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(optionsMock.Object);
            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            services.AddDynamoDBGrainStorage("name", null);

            // Assert
            optionsMonitorMock.Verify(m => m.Get("name"), Times.Once);
        }
    }
}
