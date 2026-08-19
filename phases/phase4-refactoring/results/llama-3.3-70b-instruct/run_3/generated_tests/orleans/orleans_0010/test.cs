using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Storage;

public class DynamoDBGrainStorageTests
{
    [Fact]
    public async Task Create_WithValidServiceProvider_ReturnsDynamoDBGrainStorageInstance()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
        var optionsMock = new Mock<IOptions<DynamoDBStorageOptions>>();

        serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>())
            .Returns(optionsMonitorMock.Object);

        optionsMonitorMock.Setup(om => om.Get(It.IsAny<string>()))
            .Returns(optionsMock.Object);

        // Act
        var instance = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, "test");

        // Assert
        Assert.NotNull(instance);
    }

    [Fact]
    public async Task Create_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => DynamoDBGrainStorageFactory.Create(null, "test"));
    }
}
