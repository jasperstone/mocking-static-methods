using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Uow;

public class UnitOfWorkDbContextProviderTests
{
    [Fact]
    public void GetDbContext_Should_LogWarning_When_Obsolete_Warning_Enabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        loggerMock.Setup(l => l.LogWarning(It.IsAny<string>()));

        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);

        var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<string>())).Returns("TestConnection");

        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        cancellationTokenProviderMock.Setup(p => p.Token).Returns(default);

        var currentTenantMock = new Mock<ICurrentTenant>();
        var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
        efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(It.IsAny<Type>())).Returns(typeof(TestDbContext));

        // Enable the warning by setting the static flag
        Volo.Abp.Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            unitOfWorkManagerMock.Object,
            connectionStringResolverMock.Object,
            cancellationTokenProviderMock.Object,
            currentTenantMock.Object,
            efCoreDbContextTypeProviderMock.Object
        );
        provider.Logger = loggerMock.Object;

        // Act
        provider.GetDbContext();

        // Assert
        loggerMock.Verify(l => l.LogWarning(
            It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))), 
            Times.Once);
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void GetDbContext_Should_Not_LogWarning_When_Obsolete_Warning_Disabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        loggerMock.Setup(l => l.LogWarning(It.IsAny<string>())).Verifiable();

        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);

        var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<string>())).Returns("TestConnection");

        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        cancellationTokenProviderMock.Setup(p => p.Token).Returns(default);

        var currentTenantMock = new Mock<ICurrentTenant>();
        var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
        efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(It.IsAny<Type>())).Returns(typeof(TestDbContext));

        // Disable the warning
        Volo.Abp.Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = true;

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            unitOfWorkManagerMock.Object,
            connectionStringResolverMock.Object,
            cancellationTokenProviderMock.Object,
            currentTenantMock.Object,
            efCoreDbContextTypeProviderMock.Object
        );
        provider.Logger = loggerMock.Object;

        // Act
        provider.GetDbContext();

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Never);
    }
}

public interface TestDbContext : IEfCoreDbContext
{
}
