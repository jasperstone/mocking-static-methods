using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public void CreateDbContextWithTransaction_LogsWarning_WhenTransactionsAreNotSupported()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<MyDbContext>>>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dbContextMock = new Mock<MyDbContext>();

            serviceProviderMock.Setup(sp => sp.GetRequiredService<MyDbContext>()).Returns(dbContextMock.Object);
            unitOfWorkMock.Setup(uow => uow.ServiceProvider).Returns(serviceProviderMock.Object);
            unitOfWorkMock.Setup(uow => uow.Options.IsolationLevel).Returns(IsolationLevel.ReadCommitted);

            var provider = new UnitOfWorkDbContextProvider<MyDbContext>(unitOfWorkMock.Object, null, null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            dbContextMock.Setup(db => db.Database.BeginTransaction(It.IsAny<IsolationLevel>())).Throws(new NotSupportedException());
            provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s == UnitOfWorkDbContextProvider<MyDbContext>.TransactionsNotSupportedWarningMessage)), Times.Once);
        }

        private class MyDbContext : IEfCoreDbContext
        {
            public DbContext DbContext => new DbContext(new DbContextOptionsBuilder().Options);

            public DatabaseFacade Database => DbContext.Database;

            public IServiceProvider ServiceProvider => null;

            public CancellationToken CancellationToken => default;

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(0);
            }

            public EntityEntry Attach(object entity)
            {
                return DbContext.Entry(entity);
            }

            public EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class
            {
                return DbContext.Entry(entity);
            }

            public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(0);
            }

            public int SaveChanges(bool acceptAllChangesOnSuccess)
            {
                return 0;
            }

            public int SaveChanges()
            {
                return 0;
            }

            public void Dispose()
            {
            }
        }
    }
}
