using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProvider_LoggerExtensions_Tests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenTransactionNotSupported()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<FakeEfCoreDbContext>>>();

            var provider = new UnitOfWorkDbContextProvider<FakeEfCoreDbContext>(
                unitOfWorkManager: new FakeUnitOfWorkManager(),
                connectionStringResolver: null,
                cancellationTokenProvider: null,
                currentTenant: null,
                efCoreDbContextTypeProvider: new FakeEfCoreDbContextTypeProvider()
            );
            provider.Logger = loggerMock.Object;

            // Act
            var dbContext = provider.GetDbContext();

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

        // Fake implementations to support the test

        private class FakeEfCoreDbContext : IEfCoreDbContext
        {
            public Microsoft.EntityFrameworkCore.DbContext As<T>() where T : Microsoft.EntityFrameworkCore.DbContext
            {
                return null!;
            }

            public FakeDatabaseFacade Database { get; } = new FakeDatabaseFacade();

            Microsoft.EntityFrameworkCore.DatabaseFacade IEfCoreDbContext.Database => Database;
        }

        private class FakeDatabaseFacade : Microsoft.EntityFrameworkCore.DatabaseFacade
        {
            public FakeDatabaseFacade() : base(new Microsoft.EntityFrameworkCore.DbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<Microsoft.EntityFrameworkCore.DbContext>()))
            {
            }

            public override Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction BeginTransaction()
            {
                throw new InvalidOperationException("Transactions not supported");
            }

            public override Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
            {
                throw new InvalidOperationException("Transactions not supported");
            }
        }

        private class FakeUnitOfWorkManager : IUnitOfWorkManager
        {
            public IUnitOfWork Current => new FakeUnitOfWork();
        }

        private class FakeUnitOfWork : IUnitOfWork
        {
            public IServiceProvider ServiceProvider => new FakeServiceProvider();

            public IUnitOfWorkOptions Options => new FakeUnitOfWorkOptions();

            public object? FindTransactionApi(string key) => null;

            public void AddTransactionApi(string key, object transactionApi) { }
        }

        private class FakeServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(FakeEfCoreDbContext))
                {
                    return new FakeEfCoreDbContext();
                }
                return null!;
            }
        }

        private class FakeUnitOfWorkOptions : IUnitOfWorkOptions
        {
            public bool IsTransactional => true;

            public System.Data.IsolationLevel? IsolationLevel => null;
        }

        private class FakeEfCoreDbContextTypeProvider : IEfCoreDbContextTypeProvider
        {
            public Type GetDbContextType(Type dbContextType)
            {
                return typeof(FakeEfCoreDbContext);
            }
        }

        // Interfaces from the production code (simplified)

        private interface IEfCoreDbContext
        {
            Microsoft.EntityFrameworkCore.DbContext As<T>() where T : Microsoft.EntityFrameworkCore.DbContext;
            Microsoft.EntityFrameworkCore.DatabaseFacade Database { get; }
        }

        private interface IUnitOfWorkManager
        {
            IUnitOfWork Current { get; }
        }

        private interface IUnitOfWork
        {
            IServiceProvider ServiceProvider { get; }
            IUnitOfWorkOptions Options { get; }
            object? FindTransactionApi(string key);
            void AddTransactionApi(string key, object transactionApi);
        }

        private interface IUnitOfWorkOptions
        {
            bool IsTransactional { get; }
            System.Data.IsolationLevel? IsolationLevel { get; }
        }

        private interface IEfCoreDbContextTypeProvider
        {
            Type GetDbContextType(Type dbContextType);
        }
    }
}
