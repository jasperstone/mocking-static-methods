using Moq;
using System;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.Clients;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow.MongoDB;
using Microsoft.Extensions.Logging;
using Xunit;

// Mock a class that implements IAbpMongoDbContext for testing
public class MockMongoDbContext : IAbpMongoDbContext
{
    public IMongoDatabase Database { get; set; }
    public IMongoClient Client { get; set; }
    public IMongoSession Session { get; set; }

    public IMongoCollection<T> Collection<T>(string name)
    {
        throw new NotImplementedException();
    }

    public IMongoCollection<T> Collection<T>()
    {
        throw new NotImplementedException();
    }
}

public class UnitOfWorkMongoDbContextProviderTests
{
    [Fact]
    public void GetDbContext_ShouldLogWarning_WhenObsoleteDbContextCreationWarningIsEnabled()
    {
        // Arrange
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var currentTenantMock = new Mock<ICurrentTenant>();
        var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
        var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockMongoDbContext>>>();
        var provider = new UnitOfWorkMongoDbContextProvider<MockMongoDbContext>(
            unitOfWorkManagerMock.Object,
            connectionStringResolverMock.Object,
            cancellationTokenProviderMock.Object,
            currentTenantMock.Object,
            dbContextTypeProviderMock.Object,
            mongoClientFactoryMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Enable the obsolete warning
        provider.GetType().GetProperty("EnableObsoleteDbContextCreationWarning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(provider, true);

        // Disable the global warning disable
        provider.GetType().GetProperty("DisableObsoleteDbContextCreationWarning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(provider, false);

        // Act
        provider.GetDbContext();

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }
}
