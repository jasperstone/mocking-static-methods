using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.EntityFrameworkCore;
using Volo.Abp.Uow;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var mockConnectionStringResolver = new Mock<IConnectionStringResolver>();
            var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var mockCurrentTenant = new Mock<ICurrentTenant>();
            var mockEfCoreDbContextTypeProvider = new Mock<IEfCoreDbContextTypeProvider>();

            var mockDbContext = new Mock<IEfCoreDbContext>();

            var efCoreDatabaseApi = new EfCoreDatabaseApi(mockDbContext.Object);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<object>>()))
                .Returns(efCoreDatabaseApi);

            mockUnitOfWorkManager.Setup(m => m.Current).Returns(mockUnitOfWork.Object);

            mockEfCoreDbContextTypeProvider.Setup(p => p.GetDbContextType(typeof(IEfCoreDbContext)))
                .Returns(typeof(IEfCoreDbContext));

            mockConnectionStringResolver.Setup(r => r.Resolve(It.IsAny<string>())).Returns("FakeConnectionString");

            // Setup static properties for warning flags
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
            Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning = new Lazy<bool>(() => false);

            var provider = new UnitOfWorkDbContextProvider<IEfCoreDbContext>(
                mockUnitOfWorkManager.Object,
                mockConnectionStringResolver.Object,
                mockCancellationTokenProvider.Object,
                mockCurrentTenant.Object,
                mockEfCoreDbContextTypeProvider.Object);

            var mockLogger = new Mock<ILogger<UnitOfWorkDbContextProvider<IEfCoreDbContext>>>();
            provider.Logger = mockLogger.Object;

            // Act
            var dbContext = provider.GetDbContext();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("at ")), // StackTrace contains "at "
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
