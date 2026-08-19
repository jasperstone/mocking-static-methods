using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public void CreateDbContextWithTransaction_LogsWarning_WhenTransactionNotSupported()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<FakeDbContext>>>();
            var provider = new UnitOfWorkDbContextProvider<FakeDbContext>(
                unitOfWorkManager: new FakeUnitOfWorkManager(),
                connectionStringResolver: new FakeConnectionStringResolver(),
                cancellationTokenProvider: new FakeCancellationTokenProvider(),
                currentTenant: new FakeCurrentTenant(),
                efCoreDbContextTypeProvider: new FakeEfCoreDbContextTypeProvider()
            );
            provider.Logger = loggerMock.Object;

            var unitOfWork = new FakeUnitOfWorkThatThrowsOnBeginTransaction();

            // Act
            var dbContext = provider.CreateDbContextWithTransaction(unitOfWork);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Current database does not support transactions")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Fake classes to satisfy dependencies and simulate behavior

        private class FakeDbContext : IEfCoreDbContext
        {
            public Microsoft.EntityFrameworkCore.DbContext As<T>() where T : class
            {
                return new Microsoft.EntityFrameworkCore.DbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<Microsoft.EntityFrameworkCore.DbContext>());
            }

            public Microsoft.EntityFrameworkCore.DbContext DbContext => new Microsoft.EntityFrameworkCore.DbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<Microsoft.EntityFrameworkCore.DbContext>());

            public Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database => new Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade(DbContext);
        }

        private class FakeUnitOfWorkThatThrowsOnBeginTransaction : IUnitOfWork
        {
            public IServiceProvider ServiceProvider => new FakeServiceProvider();

            public IUnitOfWorkOptions Options => new FakeUnitOfWorkOptions();

            public void AddTransactionApi(string key, object api) { }

            public object FindTransactionApi(string key) => null;

            public object FindDatabaseApi(string key) => null;

            public void AddDatabaseApi(string key, object api) { }
        }

        private class FakeServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(FakeDbContext))
                {
                    return new FakeDbContextThatThrowsOnBeginTransaction();
                }
                return null;
            }
        }

        private class FakeDbContextThatThrowsOnBeginTransaction : FakeDbContext
        {
            public new Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database => new FakeDatabaseFacade();

            private class FakeDatabaseFacade : Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade
            {
                public FakeDatabaseFacade() : base(new Microsoft.EntityFrameworkCore.DbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<Microsoft.EntityFrameworkCore.DbContext>())) { }

                public override Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction BeginTransaction()
                {
                    throw new InvalidOperationException();
                }

                public override Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private class FakeUnitOfWorkOptions : IUnitOfWorkOptions
        {
            public bool IsTransactional => true;

            public System.Data.IsolationLevel? IsolationLevel => null;
        }

        private class FakeUnitOfWorkManager : IUnitOfWorkManager
        {
            public IUnitOfWork Current => new FakeUnitOfWorkThatThrowsOnBeginTransaction();
        }

        private class FakeConnectionStringResolver : Volo.Abp.Data.IConnectionStringResolver
        {
            public string Resolve(string connectionStringName) => "FakeConnectionString";

            public System.Threading.Tasks.Task<string> ResolveAsync(string connectionStringName) => System.Threading.Tasks.Task.FromResult("FakeConnectionString");
        }

        private class FakeCancellationTokenProvider : Volo.Abp.Threading.ICancellationTokenProvider
        {
            public System.Threading.CancellationToken Token => System.Threading.CancellationToken.None;
        }

        private class FakeCurrentTenant : Volo.Abp.MultiTenancy.ICurrentTenant
        {
            public Guid? Id => null;

            public IDisposable Change(Guid? tenantId) => null;
        }

        private class FakeEfCoreDbContextTypeProvider : Volo.Abp.EntityFrameworkCore.IEfCoreDbContextTypeProvider
        {
            public Type GetDbContextType(Type dbContextType) => typeof(FakeDbContext);
        }
    }
}
