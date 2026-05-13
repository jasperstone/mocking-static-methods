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
            dbContextMock.SetupGet(d => d.Database).Returns(databaseMock.Object);

            // Setup BeginTransaction to throw InvalidOperationException to trigger catch block
            databaseMock.Setup(d => d.BeginTransaction(It.IsAny<IsolationLevel>()))
                .Throws(new InvalidOperationException());
            databaseMock.Setup(d => d.BeginTransaction())
                .Throws(new InvalidOperationException());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(TestDbContext)))
                .Returns(dbContextMock.Object);

            var unitOfWorkOptions = new TestUnitOfWorkOptions
            {
                IsTransactional = true,
                IsolationLevel = null
            };

            var unitOfWork = new TestUnitOfWork(serviceProviderMock.Object, unitOfWorkOptions);

            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            unitOfWorkManagerMock.SetupGet(m => m.Current).Returns(unitOfWork);

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
            var connectionStringName = "FakeConnectionStringName";

            // We need to set DbContextCreationContext.Current to a context with the connection string
            // This is internal to the library, so we simulate by calling CreateDbContext with the context set
            // But since CreateDbContextWithTransaction uses DbContextCreationContext.Current.ConnectionString,
            // we simulate by setting it manually via reflection or by calling CreateDbContext with context

            // Instead, we call CreateDbContext with a context set by calling CreateDbContext with parameters
            // We will call CreateDbContextWithTransaction directly, but we need to set DbContextCreationContext.Current

            // Use reflection to set DbContextCreationContext.Current
            var creationContextType = typeof(UnitOfWorkDbContextProvider<TestDbContext>).Assembly.GetType("Volo.Abp.Uow.EntityFrameworkCore.DbContextCreationContext");
            var useMethod = creationContextType.GetMethod("Use", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var ctor = creationContextType.GetConstructor(new[] { typeof(string), typeof(string) });
            using (var context = (IDisposable)ctor.Invoke(new object[] { connectionStringName, connectionString }))
            {
                // Set current context
                var currentProperty = creationContextType.GetProperty("Current", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                currentProperty.SetValue(null, context);

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
