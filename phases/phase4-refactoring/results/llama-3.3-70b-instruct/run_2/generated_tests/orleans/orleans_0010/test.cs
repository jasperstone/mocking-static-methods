using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;
using System;

public class DynamoDBGrainStorageFactoryTests
{
    [Fact]
    public void Create_WithValidServiceProvider_ReturnsDynamoDBGrainStorage()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var optionsMonitorMock = new Mock<Microsoft.Extensions.Options.IOptionsMonitor<Orleans.Storage.DynamoDBStorageOptions>>();
        var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<Orleans.Storage.DynamoDBStorageOptions>>();

        optionsMock.SetupGet(o => o.Value).Returns(new Orleans.Storage.DynamoDBStorageOptions());
        optionsMonitorMock.Setup(om => om.Get(It.IsAny<string>())).Returns(optionsMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(Microsoft.Extensions.Options.IOptionsMonitor<Orleans.Storage.DynamoDBStorageOptions>))).Returns(optionsMonitorMock.Object);

        // Act
        var grainStorage = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, "test");

        // Assert
        Assert.NotNull(grainStorage);
    }

    [Fact]
    public void Create_WithInvalidServiceProvider_ThrowsException()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(Microsoft.Extensions.Options.IOptionsMonitor<Orleans.Storage.DynamoDBStorageOptions>))).Returns(null);

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, "test"));
    }
}
