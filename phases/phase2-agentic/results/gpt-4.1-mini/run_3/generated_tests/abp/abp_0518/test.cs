using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
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
            private readonly Mock<IServiceScope> _serviceScopeMock;

            private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;

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

            // Setup Database property to return our mock
            dbContextMock.SetupGet(d => d.Database).Returns(databaseMock.Object);

            // Setup BeginTransaction to throw InvalidOperationException to trigger catch block
            databaseMock.Setup(d => d.BeginTransaction(It.IsAny<IsolationLevel>()))
                .Throws(new InvalidOperationException());

            databaseMock.Setup(d => d.BeginTransaction())
                .Throws(new InvalidOperationException());

            // Setup ServiceProvider to return our dbContextMock.Object
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(TestDbContext)))
                .Returns(dbContextMock.Object);

            var unitOfWorkOptions = new TestUnitOfWorkOptions
            {
                IsTransactional = true,
                IsolationLevel = null
            };

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            unitOfWorkMock.SetupGet(u => u.Options).Returns(unitOfWorkOptions);
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(null);
            unitOfWorkMock.Setup(u => u.AddTransactionApi(It.IsAny<string>(), It.IsAny<object>()));

            // Setup DbContextCreationContext.Current.ConnectionString for transactionApiKey
            var connectionString = "FakeConnectionString";
            var creationContext = new DbContextCreationContext("FakeConnectionName", connectionString);
            using (DbContextCreationContext.Use(creationContext))
            {
                var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
                    Mock.Of<IUnitOfWorkManager>(),
                    Mock.Of<IConnectionStringResolver>(),
                    Mock.Of<ICancellationTokenProvider>(),
                    Mock.Of<ICurrentTenant>(),
                    Mock.Of<IEfCoreDbContextTypeProvider>()
                )
                {
                    Logger = loggerMock.Object
                };

                // Act
                var result = provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

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
