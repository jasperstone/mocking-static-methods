using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.EntityFrameworkCore;
using Volo.Abp.Uow;
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
            private readonly Mock<IServiceScope> _serviceScopeMock;

            private readonly System.Collections.Generic.Dictionary<string, object> _transactionApis = new();

            public TestUnitOfWork(IServiceProvider serviceProvider, IUnitOfWorkOptions options)
            {
                ServiceProvider = serviceProvider;
                Options = options;
            }

            public object FindTransactionApi(string key)
            {
                _transactionApis.TryGetValue(key, out var api);
                return api;
            }

            public void AddTransactionApi(string key, object api)
            {
                _transactionApis[key] = api;
            }

            public object FindDatabaseApi(string key) => null;
            public object GetOrAddDatabaseApi(string key, Func<object> factory) => factory();
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
            var databaseMock = new Mock<DatabaseFacade>(dbContext);
            var dbTransactionMock = new Mock<IDbContextTransaction>();

            // Setup Database property to return our mock
            dbContextMock.SetupGet(d => d.Database).Returns(databaseMock.Object);

            // Setup BeginTransaction to throw InvalidOperationException to trigger catch block
            databaseMock.Setup(d => d.BeginTransaction(It.IsAny<IsolationLevel>())).Throws<InvalidOperationException>();
            databaseMock.Setup(d => d.BeginTransaction()).Throws<InvalidOperationException>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(TestDbContext))).Returns(dbContextMock.Object);

            var unitOfWorkOptions = new TestUnitOfWorkOptions { IsTransactional = true, IsolationLevel = null };
            var unitOfWork = new TestUnitOfWork(serviceProviderMock.Object, unitOfWorkOptions);

            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

            var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object
            );

            provider.Logger = loggerMock.Object;

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
            }
        }
    }
}
