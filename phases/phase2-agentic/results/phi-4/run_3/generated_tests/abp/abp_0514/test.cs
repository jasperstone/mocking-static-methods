using Moq;
using System;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

public class UnitOfWorkDbContextProviderTests
{
    [Fact]
    public void GetDbContext_ShouldLogWarning_WhenObsoleteMethodIsCalled()
    {
        // Arrange
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var currentTenantMock = new Mock<ICurrentTenant>();
        var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<object>>>();

        var provider = new UnitOfWorkDbContextProvider<object>(
            unitOfWorkManagerMock.Object,
            connectionStringResolverMock.Object,
            cancellationTokenProviderMock.Object,
            currentTenantMock.Object,
            efCoreDbContextTypeProviderMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Set up the condition to trigger the warning
        provider.UnitOfWorkManager = new UnitOfWorkManager(new UnitOfWorkOptions(), new ServiceCollection().BuildServiceProvider());
        provider.UnitOfWorkManager.Current = new UnitOfWork(new UnitOfWorkOptions());

        // Act
        provider.GetDbContext();

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated. Use GetDbContextAsync instead!")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );

        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<string>(),
                It.IsAny<object[]>()
            ),
            Times.Once // This verifies the second LogWarning call
        );
    }
}
