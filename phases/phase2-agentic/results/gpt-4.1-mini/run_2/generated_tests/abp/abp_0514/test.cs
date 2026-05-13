using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private class DummyDbContext : IEfCoreDbContext { }

        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningEnabledAndNotDisabled()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<object>>()))
                .Returns((string key, Func<object> factory) => factory());

            unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);

            // Setup static properties to enable warning
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
            Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

            efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(typeof(DummyDbContext)))
                .Returns(typeof(DummyDbContext));
            connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<string>())).Returns("connString");

            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<DummyDbContext>>>();

            var provider = new UnitOfWorkDbContextProvider<DummyDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            // Act
            // We expect an exception because CreateDbContext is not fully mocked, so catch it
            try
            {
                provider.GetDbContext();
            }
            catch
            {
                // ignored
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Length > 0),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }

    // Static classes to simulate static properties used in the tested code
    public static class UnitOfWork
    {
        public static bool EnableObsoleteDbContextCreationWarning { get; set; }
    }

    public static class Uow
    {
        public static class UnitOfWorkManager
        {
            public static ThreadLocal<bool> DisableObsoleteDbContextCreationWarning { get; } = new ThreadLocal<bool>(() => false);
        }
    }
}
