using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Storage;
using System;

public class DynamoDBGrainStorageFactoryTests
{
    [Fact]
    public void Create_WithValidServiceProvider_ReturnsDynamoDBGrainStorage()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<DynamoDBStorageOptions>();
        var serviceProvider = services.BuildServiceProvider();

        var optionsMonitor = Mock.Of<IOptionsMonitor<DynamoDBStorageOptions>>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(p => p.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>))).Returns(optionsMonitor);

        var name = "TestName";

        // Act
        var result = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, name);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<DynamoDBGrainStorage>(result);
    }

    [Fact]
    public void Create_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => DynamoDBGrainStorageFactory.Create(null, "TestName"));
    }

    [Fact]
    public void Create_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<DynamoDBStorageOptions>();
        var serviceProvider = services.BuildServiceProvider();

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => DynamoDBGrainStorageFactory.Create(serviceProvider, null));
    }
}
