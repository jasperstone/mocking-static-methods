using Moq;
using System;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;
using Microsoft.Extensions.Logging;

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
