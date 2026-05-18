using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private class TestDbContext : DbContext, IEfCoreDbContext
        {
            public TestDbContext(DbContextOptions options) : base(options) { }
        }

        private class TestUnitOfWorkOptions : IAbpUnitOfWorkOptions
        {
            public bool IsTransactional { get; set; }
            public IsolationLevel? IsolationLevel { get; set; }
            public int? Timeout { get; set; }
        }

        private class TestUnitOfWork : IUnitOfWork
        {
            public IServiceProvider ServiceProvider { get; }
            public IAbpUnitOfWorkOptions Options { get; }
            private readonly Func<string, object> _findTransactionApi;
            private readonly Action<string, object> _addTransactionApi;

            public TestUnitOfWork(IServiceProvider serviceProvider, IAbpUnitOfWorkOptions options,
                Func<string, object> findTransactionApi = null,
                Action<string, object> addTransactionApi = null)
            {
                ServiceProvider = serviceProvider;
                Options = options;
                _findTransactionApi = findTransactionApi;
                _addTransactionApi = addTransactionApi;
                Items = new System.Collections.Generic.Dictionary<string, object>();
            }

            public object FindTransactionApi(string key) => _findTransactionApi?.Invoke(key);

            public void AddTransactionApi(string key, object api) => _addTransactionApi?.Invoke(key, api);

            // Implement other members of IUnitOfWork with minimal stubs
            public Guid Id { get; } = Guid.NewGuid();
            public System.Collections.Generic.Dictionary<string, object> Items { get; }
            public event EventHandler<UnitOfWorkFailedEventArgs> Failed;
            public event EventHandler<UnitOfWorkEventArgs> Disposed;
            public IUnitOfWork Outer => null;
            public bool IsReserved => false;
            public bool IsDisposed => false;
            public bool IsCompleted => false;
            public string ReservationName => null;
            public void SetOuter(IUnitOfWork outer) { }
            public void Initialize(AbpUnitOfWorkOptions options) { }
            public void Reserve(string reservationName) { }
            public System.Threading.Tasks.Task SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.CompletedTask;
            public System.Threading.Tasks.Task CompleteAsync(System.Threading.CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.CompletedTask;
            public System.Threading.Tasks.Task RollbackAsync(System.Threading.CancellationToken cancellationToken = default) => System.Threading.Tasks.Task.CompletedTask;
            public void OnCompleted(Func<System.Threading.Tasks.Task> handler) { }
            public void AddOrReplaceLocalEvent(UnitOfWorkEventRecord eventRecord, Predicate<UnitOfWorkEventRecord> replacementSelector = null) { }
            public void AddOrReplaceDistributedEvent(UnitOfWorkEventRecord eventRecord, Predicate<UnitOfWorkEventRecord> replacementSelector = null) { }
            public object FindDatabaseApi(string key) => null;
            public object GetOrAddDatabaseApi(string key, Func<object> factory) => null;
            public void AddDatabaseApi(string key, object api) { }
        }

        [Fact]
        public void CreateDbContextWithTransaction_LogsWarning_WhenTransactionNotSupported()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();

            var dbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("TestDb").Options;
            var dbContext = new TestDbContext(dbContextOptions);

            var dbContextMock = new Mock<TestDbContext>(dbContextOptions) { CallBase = true };
            var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            dbContextMock.SetupGet(d => d.Database).Returns(databaseMock.Object);

            // Setup BeginTransaction to throw InvalidOperationException
            databaseMock.Setup(d => d.BeginTransaction(It.IsAny<IsolationLevel>()))
                .Throws(new InvalidOperationException());
            databaseMock.Setup(d => d.BeginTransaction())
                .Throws(new InvalidOperationException());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(TestDbContext))).Returns(dbContextMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(TestDbContext))).Returns(dbContextMock.Object);

            var options = new TestUnitOfWorkOptions { IsTransactional = true, IsolationLevel = null };

            EfCoreTransactionApi capturedTransactionApi = null;
            var unitOfWork = new TestUnitOfWork(serviceProviderMock.Object, options,
                findTransactionApi: key => null,
                addTransactionApi: (key, api) => capturedTransactionApi = api as EfCoreTransactionApi);

            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

            var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
                unitOfWorkManager: null,
                connectionStringResolver: null,
                cancellationTokenProvider: cancellationTokenProviderMock.Object,
                currentTenant: null,
                efCoreDbContextTypeProvider: null)
            {
                Logger = loggerMock.Object
            };

            // Setup DbContextCreationContext.Current.ConnectionString for transactionApiKey
            var connectionString = "FakeConnectionString";
            var creationContext = new DbContextCreationContext("Default", connectionString);
            using (DbContextCreationContext.Use(creationContext))
            {
                // Act
                var result = provider.CreateDbContextWithTransaction(unitOfWork);

                // Assert
                Assert.Same(dbContextMock.Object, result);
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Current database does not support transactions")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
                Assert.Null(capturedTransactionApi);
            }
        }
    }
}
