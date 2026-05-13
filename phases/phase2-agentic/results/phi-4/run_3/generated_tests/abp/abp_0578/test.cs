using Moq;
using System;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow.MongoDB;
using Volo.Abp.Uow;
using Microsoft.Extensions.Logging;
using Xunit;

public class UnitOfWorkMongoDbContextProviderTests
{
    [Fact]
    public void GetDbContext_ShouldLogWarning_WhenObsoleteDbContextCreationWarningIsEnabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockMongoDbContext>>>();
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var connectionStringResolverMock = Mock.Of<IConnectionStringResolver>();
        var cancellationTokenProviderMock = Mock.Of<ICancellationTokenProvider>();
        var currentTenantMock = Mock.Of<ICurrentTenant>();
        var dbContextTypeProviderMock = Mock.Of<IMongoDbContextTypeProvider>();
        var mongoClientFactoryMock = Mock.Of<IAbpMongoClientFactory>();

        var provider = new UnitOfWorkMongoDbContextProvider<MockMongoDbContext>(
            unitOfWorkManagerMock.Object,
            connectionStringResolverMock,
            cancellationTokenProviderMock,
            currentTenantMock,
            dbContextTypeProviderMock,
            mongoClientFactoryMock)
        {
            Logger = loggerMock.Object
        };

        // Enable the obsolete warning
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        unitOfWorkManagerMock.Setup(uow => uow.DisableObsoleteDbContextCreationWarning).Returns(false);

        // Act
        provider.GetDbContext();

        // Assert
        loggerMock.Verify(
            logger => logger.LogWarning(
                It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                It.IsAny<Exception>()
            ),
            Times.Once
        );
    }
}

// Mock classes for testing
public class MockMongoDbContext : IAbpMongoDbContext
{
    public void InitializeDatabase(IMongoDatabase database, MongoClient client, object options)
    {
        // Mock implementation
    }
}
