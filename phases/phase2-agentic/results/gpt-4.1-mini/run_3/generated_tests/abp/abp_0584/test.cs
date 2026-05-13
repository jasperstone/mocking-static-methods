using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow.MongoDB;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        private class TestMongoDbContext : IAbpMongoDbContext
        {
            public bool Initialized { get; private set; }
            public IMongoDatabase Database { get; private set; }
            public MongoClient Client { get; private set; }
            public IClientSessionHandle Session { get; private set; }

            public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle session)
            {
                Initialized = true;
                Database = database;
                Client = client;
                Session = session;
            }
        }

        private class TestUnitOfWork : IUnitOfWork
        {
            public IServiceProvider ServiceProvider { get; }
            public IUnitOfWorkOptions Options { get; }
            private readonly System.Collections.Generic.Dictionary<string, object> _transactionApis = new();
            private readonly System.Collections.Generic.Dictionary<string, object> _databaseApis = new();

            public TestUnitOfWork(IServiceProvider serviceProvider, IUnitOfWorkOptions options)
            {
                ServiceProvider = serviceProvider;
                Options = options;
            }

            public void AddTransactionApi(string key, object transactionApi)
            {
                _transactionApis[key] = transactionApi;
            }

            public object FindTransactionApi(string key)
            {
                _transactionApis.TryGetValue(key, out var api);
                return api;
            }

            public void AddDatabaseApi(string key, object databaseApi)
            {
                _databaseApis[key] = databaseApi;
            }

            public object FindDatabaseApi(string key)
            {
                _databaseApis.TryGetValue(key, out var api);
                return api;
            }
        }

        private class TestUnitOfWorkOptions : IUnitOfWorkOptions
        {
            public bool IsTransactional { get; set; }
            public int? Timeout { get; set; }
        }

        private class TestMongoDbTransactionApi
        {
            public IClientSessionHandle SessionHandle { get; }
            public TestMongoDbTransactionApi(IClientSessionHandle sessionHandle)
            {
                SessionHandle = sessionHandle;
            }
        }

        [Fact]
        public async Task CreateDbContextWithTransactionAsync_LogsWarning_WhenStartTransactionThrowsNotSupportedException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var dbContext = new TestMongoDbContext();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(TestMongoDbContext))).Returns(dbContext);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<TestMongoDbContext>()).Returns(dbContext);

            var unitOfWorkOptions = new TestUnitOfWorkOptions { IsTransactional = true };
            var unitOfWork = new TestUnitOfWork(serviceProviderMock.Object, unitOfWorkOptions);

            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            unitOfWorkManagerMock.SetupGet(m => m.Current).Returns(unitOfWork);

            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            dbContextTypeProviderMock.Setup(m => m.GetDbContextType(typeof(TestMongoDbContext))).Returns(typeof(TestMongoDbContext));
            var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

            var mongoUrl = new MongoUrl("mongodb://localhost:27017/testdb");
            var mongoClientMock = new Mock<MongoClient>(MockBehavior.Strict, new object[] { mongoUrl });
            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            mongoClientMock.Setup(c => c.GetDatabase("testdb", null)).Returns(mongoDatabaseMock.Object);

            var clientSessionHandleMock = new Mock<IClientSessionHandle>();
            clientSessionHandleMock.Setup(s => s.AdvanceOperationTime(It.IsAny<BsonTimestamp>()));
            clientSessionHandleMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            mongoClientMock.Setup(c => c.StartSession(It.IsAny<ClientSessionOptions>(), default)).Returns(clientSessionHandleMock.Object);

            mongoClientFactoryMock.Setup(f => f.Get(mongoUrl)).Returns(mongoClientMock.Object);

            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                dbContextTypeProviderMock.Object,
                mongoClientFactoryMock.Object
            );
            provider.Logger = loggerMock.Object;

            // Act
            var result = await provider.CreateDbContextWithTransactionAsync(
                unitOfWork,
                mongoUrl,
                mongoClientMock.Object,
                mongoDatabaseMock.Object,
                CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Current database does not support transactions")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Same(dbContext, result);
            Assert.True(dbContext.Initialized);
            Assert.Null(dbContext.Session);
            Assert.Same(mongoDatabaseMock.Object, dbContext.Database);
            Assert.Same(mongoClientMock.Object, dbContext.Client);
        }
    }
}
