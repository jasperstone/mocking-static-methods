using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Transactions.Abstractions;
using Orleans.Transactions.AzureStorage;
using Orleans.Hosting;
using Xunit;

public class AzureTableTransactionServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureTableTransactionalStateStorage_CallsGetRequiredServiceWithCorrectType()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableTransactionalStateOptions>>();

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>())
            .Returns(optionsMonitorMock.Object);

        var optionsBuilderMock = new Mock<OptionsBuilder<AzureTableTransactionalStateOptions>>();
        services.AddOptions<AzureTableTransactionalStateOptions>("testName").Configure(options => { });

        // Act
        services.AddAzureTableTransactionalStateStorage("testName");

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableTransactionalStateOptions>>(), Times.Once);
    }

    [Fact]
    public void AddAzureTableTransactionalStateStorage_AddsExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureTableTransactionalStateStorage("testName");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.IsType<AzureTableTransactionalStateOptionsValidator>(serviceProvider.GetRequiredService<IConfigurationValidator>());
        Assert.IsType<ITransactionalStateStorageFactory>(serviceProvider.GetRequiredService<ITransactionalStateStorageFactory>());
        Assert.IsType<ILifecycleParticipant<ISiloLifecycle>>(serviceProvider.GetRequiredService<ILifecycleParticipant<ISiloLifecycle>>());
    }
}
