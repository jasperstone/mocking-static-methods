using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransaction_LogsWarning_WhenTransactionsAreNotSupported()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<MyDbContext>>>();
            var dbContextProvider = new UnitOfWorkDbContextProvider<MyDbContext>(unitOfWorkManagerMock.Object, null, null, null, null);
            dbContextProvider.Logger = loggerMock.Object;

            unitOfWorkManagerMock.Setup(u => u.Current).Returns(unitOfWorkMock.Object);
            unitOfWorkMock.Setup(u => u.Options.IsTransactional).Returns(true);
            unitOfWorkMock.Setup(u => u.Options.IsolationLevel).Returns(IsolationLevel.ReadCommitted);

            var dbContextMock = new Mock<MyDbContext>();
            dbContextMock.Setup(d => d.Database.BeginTransaction(It.IsAny<IsolationLevel>())).Throws(new NotSupportedException());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetService(typeof(MyDbContext))).Returns(dbContextMock.Object);
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);

            // Act
            var dbContext = dbContextProvider.CreateDbContext(unitOfWorkMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s == "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.")), Times.Once);
        }
    }

    public class MyDbContext : DbContext, IEfCoreDbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public void Initialize(AbpEfCoreDbContextInitializationContext context)
        {
        }

        public void Attach<TEntity>(TEntity entity) where TEntity : class
        {
            base.Attach(entity);
        }

        public void Attach(object entity)
        {
            base.Attach(entity);
        }

        public int SaveChanges()
        {
            return base.SaveChanges();
        }

        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }

    public class MyEntity
    {
    }
}
