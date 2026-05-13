using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransaction_LogsWarning_WhenTransactionIsNotSupported()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<MyDbContext>>>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dbContextMock = new Mock<MyDbContext>();

            unitOfWorkMock.Setup(u => u.Options.IsTransactional).Returns(true);
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(s => s.GetService(typeof(MyDbContext))).Returns(dbContextMock.Object);

            var provider = new UnitOfWorkDbContextProvider<MyDbContext>(
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<IConnectionStringResolver>(),
                Mock.Of<ICancellationTokenProvider>(),
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IEfCoreDbContextTypeProvider>()
            )
            {
                Logger = loggerMock.Object
            };

            dbContextMock.Setup(d => d.Database.BeginTransaction(It.IsAny<IsolationLevel>())).Throws(new NotSupportedException());

            // Act
            provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s == UnitOfWorkDbContextProvider<MyDbContext>.TransactionsNotSupportedWarningMessage)), Times.Once);
        }

        private class MyDbContext : IEfCoreDbContext
        {
            public DbSet<MyEntity> MyEntities { get; set; }

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }

        private class MyEntity
        {
        }
    }
}
