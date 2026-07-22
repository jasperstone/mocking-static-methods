using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private const string TransactionsNotSupportedWarningMessage = "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.";

        [Fact]
        public void CreateDbContextWithTransaction_WhenTransactionNotSupported_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UnitOfWorkDbContextProvider<IEfCoreDbContext>>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockDbContext = new Mock<IEfCoreDbContext>();
            var mockDbContextTransaction = new Mock<IDbContextTransaction>();
            var mockDbTransaction = new Mock<DbTransaction>();
            var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();

            mockUnitOfWork.Setup(uow => uow.ServiceProvider.GetRequiredService<IEfCoreDbContext>()).Returns(mockDbContext.Object);
            mockDbContext.Setup(db => db.Database.BeginTransaction(It.IsAny<IsolationLevel>())).Throws<InvalidOperationException>();
            mockDbContext.Setup(db => db.Database.BeginTransaction()).Throws<InvalidOperationException>();

            var unitOfWorkDbContextProvider = new TestableUnitOfWorkDbContextProvider(
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<IConnectionStringResolver>(),
                mockCancellationTokenProvider.Object,
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IEfCoreDbContextTypeProvider>()
            )
            {
                Logger = mockLogger.Object
            };

            // Act
            unitOfWorkDbContextProvider.CreateDbContextWithTransaction(mockUnitOfWork.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(TransactionsNotSupportedWarningMessage),
                Times.Once
            );
        }

        private class TestableUnitOfWorkDbContextProvider : UnitOfWorkDbContextProvider<IEfCoreDbContext>
        {
            public TestableUnitOfWorkDbContextProvider(
                IUnitOfWorkManager unitOfWorkManager,
                IConnectionStringResolver connectionStringResolver,
                ICancellationTokenProvider cancellationTokenProvider,
                ICurrentTenant currentTenant,
                IEfCoreDbContextTypeProvider efCoreDbContextTypeProvider)
                : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, efCoreDbContextTypeProvider)
            {
            }

            public new IEfCoreDbContext CreateDbContextWithTransaction(IUnitOfWork unitOfWork)
            {
                return base.CreateDbContextWithTransaction(unitOfWork);
            }
        }
    }
}
