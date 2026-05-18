using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Persistence.DynamoDB;
using Xunit;

namespace Orleans.Storage.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldCallGetRequiredServiceWithCorrectType()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>())
                .Returns(mockOptionsMonitor.Object);

            // Act
            var result = DynamoDBGrainStorageFactory.Create(mockServiceProvider.Object, "TestName");

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>(), Times.Once);
            Assert.NotNull(result);
        }
    }
}
