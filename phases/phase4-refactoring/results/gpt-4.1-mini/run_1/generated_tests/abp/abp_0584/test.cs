using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Volo.Abp.Uow.MongoDB;
using Volo.Abp.Uow;
using Volo.Abp.MongoDB;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        private class TestMongoDbContext : IAbpMongoDbContext
        {
            public IMongoClient Client { get; private set; }
            public IMongoDatabase Database { get; private set; }
            public IClientSessionHandle? SessionHandle { get; private set; }
            public bool Initialized { get; private set; }

            public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle? session)
            {
                Initialized = true;
                Database = database;
                Client = client;
                SessionHandle = session;
            }

            public IMongoCollection<T> Collection<T>()
            {
                return Database.GetCollection<T>("TestCollection");
            }
        }

        private class TestUnitOfWorkOptions : IUnitOfWorkOptions
        {
            public TimeSpan? Timeout { get; set; }
            public bool IsTransactional { get; set; }
        }

        private class TestUnitOfWork : IUnitOfWork
        {
            public IServiceProvider ServiceProvider { get; set; }
            public IUnitOfWorkOptions Options { get; set; }
            public void AddTransactionApi(string key, ITransactionApi transactionApi) { }
            public ITransactionApi FindTransactionApi(string key) => null!;
            public void AddDatabaseApi(string key, IDatabaseApi databaseApi) { }
            public IDatabaseApi? FindDatabaseApi(string key) => null;
        }

        private class TestMongoDbContextProvider : UnitOfWorkMongoDbContextProvider<TestMongoDbContext>
        {
            public TestMongoDbContextProvider(
                IUnitOfWorkManager unitOfWorkManager,
                IConnectionStringResolver connectionStringResolver,
                ICancellationTokenProvider cancellationTokenProvider,
                ICurrentTenant currentTenant,
                IMongoDbContextTypeProvider dbContextTypeProvider,
                IAbpMongoClientFactory mongoClientFactory)
                : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, dbContextTypeProvider, mongoClientFactory)
            {
            }

            public async Task<TestMongoDbContext> CallCreateDbContextWithTransactionAsync(
                IUnitOfWork unitOfWork,
                MongoUrl url,
                MongoClient client,
                IMongoDatabase database,
                CancellationToken cancellationToken = default)
            {
                return await base.CreateDbContextWithTransactionAsync(unitOfWork, url, client, database, cancellationToken);
            }
        }

        [Fact]
        public async Task CreateDbContextWithTransactionAsync_LogsWarning_WhenStartTransactionThrowsNotSupportedException()
        {
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var dbContext = new TestMongoDbContext();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(TestMongoDbContext))).Returns(dbContext);

            var options = new TestUnitOfWorkOptions { Timeout = null, IsTransactional = true };

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(uow => uow.ServiceProvider).Returns(serviceProviderMock.Object);
            unitOfWorkMock.Setup(uow => uow.Options).Returns(options);
            unitOfWorkMock.Setup(uow => uow.FindTransactionApi(It.IsAny<string>())).Returns((ITransactionApi?)null);

            var sessionMock = new Mock<IClientSessionHandle>();
            sessionMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            var clientMock = new Mock<MongoClient>(MockBehavior.Strict, new object[] { new MongoClientSettings() });
            clientMock.Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionMock.Object);

            var databaseMock = new Mock<IMongoDatabase>();

            var provider = new TestMongoDbContextProvider(
                unitOfWorkManager: null,
                connectionStringResolver: null,
                cancellationTokenProvider: null,
                currentTenant: null,
                dbContextTypeProvider: null,
                mongoClientFactory: null
            );
            provider.Logger = loggerMock.Object;

            var result = await provider.CallCreateDbContextWithTransactionAsync(unitOfWorkMock.Object, new MongoUrl("mongodb://localhost/testdb"), clientMock.Object, databaseMock.Object);

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
            Assert.Null(dbContext.SessionHandle);
        }
    }
}
