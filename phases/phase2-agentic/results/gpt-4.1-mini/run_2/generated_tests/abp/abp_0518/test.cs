using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
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

        private class TestUnitOfWorkOptions : IUnitOfWorkOptions
        {
            public bool IsTransactional { get; set; }
            public IsolationLevel? IsolationLevel { get; set; }
        }

        private class TestUnitOfWork : IUnitOfWork
        {
            public IServiceProvider ServiceProvider { get; }
            public IUnitOfWorkOptions Options { get; }
            private readonly Mock<IUnitOfWork> _mock;

            private readonly Mock<IServiceProvider> _serviceProviderMock;

            public TestUnitOfWork(IServiceProvider serviceProvider, IUnitOfWorkOptions options)
            {
                ServiceProvider = serviceProvider;
                Options = options;
            }

            public object FindTransactionApi(string key) => null;
            public void AddTransactionApi(string key, object transactionApi) { }
            public object FindDatabaseApi(string key) => null;
            public object GetOrAddDatabaseApi(string key, Func<object> factory) => factory();
            public void AddDatabaseApi(string key, object databaseApi) { }
        }

        [Fact]
        public void CreateDbContextWithTransaction_LogsWarning_WhenTransactionNotSupported()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();

            var dbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            var dbContextMock = new Mock<TestDbContext>(dbContextOptions) { CallBase = true };

            var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            var dbTransactionMock = new Mock<IDbContextTransaction>();

            // Setup BeginTransaction to throw InvalidOperationException to trigger catch block
            databaseMock.Setup(d => d.BeginTransaction(It.IsAny<IsolationLevel>()))
                .Throws(new InvalidOperationException());
            databaseMock.Setup(d => d.BeginTransaction())
                .Throws(new InvalidOperationException());

            dbContextMock.SetupGet(d => d.Database).Returns(databaseMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(TestDbContext)))
                .Returns(dbContextMock.Object);

            var options = new TestUnitOfWorkOptions { IsTransactional = true, IsolationLevel = null };
            var unitOfWork = new TestUnitOfWork(serviceProviderMock.Object, options);

            var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
                unitOfWorkManager: null,
                connectionStringResolver: null,
                cancellationTokenProvider: null,
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
                var resultDbContext = provider.CreateDbContextWithTransaction(unitOfWork);

                // Assert
                Assert.Same(dbContextMock.Object, resultDbContext);
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Current database does not support transactions")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }
}
