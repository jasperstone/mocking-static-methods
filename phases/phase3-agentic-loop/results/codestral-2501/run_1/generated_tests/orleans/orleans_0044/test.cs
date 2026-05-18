using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Transactions.Abstractions;
using Xunit;

public class AzureTableTransactionServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureTableTransactionalStateStorage_ShouldRegisterServices()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();
        optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new AzureTableTransactionalStateOptions());

        var transactionalStateStorageFactoryMock = new Mock<ITransactionalStateStorageFactory>();

        serviceCollection.AddSingleton(optionsMonitorMock.Object);
        serviceCollection.AddSingleton(transactionalStateStorageFactoryMock.Object);

        // Act
        serviceCollection.AddAzureTableTransactionalStateStorage("TestStorage");

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var configurationValidator = serviceProvider.GetRequiredService<IConfigurationValidator>();
        var transactionalStateStorageFactory = serviceProvider.GetRequiredService<ITransactionalStateStorageFactory>();

        Assert.NotNull(configurationValidator);
        Assert.NotNull(transactionalStateStorageFactory);
    }
}
