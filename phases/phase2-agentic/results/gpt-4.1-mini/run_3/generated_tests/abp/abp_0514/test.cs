using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.EntityFrameworkCore;
using Volo.Abp.Uow;
using Volo.Abp.Data;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private class DummyDbContext : IEfCoreDbContext { }

        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<object>>()))
                .Returns(new EfCoreDatabaseApi(new DummyDbContext()));

            unitOfWorkManagerMock.Setup(u => u.Current).Returns(unitOfWorkMock.Object);

            // Setup to enable obsolete warning
            typeof(UnitOfWork).GetProperty(nameof(UnitOfWork.EnableObsoleteDbContextCreationWarning))!.SetValue(null, true);
            typeof(Uow.UnitOfWorkManager).GetProperty(nameof(Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning))!.SetValue(null, new Lazy<bool>(() => false));

            efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(typeof(DummyDbContext))).Returns(typeof(DummyDbContext));
            connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<string>())).Returns("connString");

            var provider = new UnitOfWorkDbContextProvider<DummyDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object);

            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<DummyDbContext>>>();
            provider.Logger = loggerMock.Object;

            // Act
            var dbContext = provider.GetDbContext();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("at ")), // StackTrace contains "at "
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
