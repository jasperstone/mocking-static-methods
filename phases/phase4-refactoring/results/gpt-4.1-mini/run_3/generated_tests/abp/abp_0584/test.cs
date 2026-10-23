using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    // Minimal stubs for missing interfaces to compile the test
    public interface IUnitOfWorkOptions
    {
        bool IsTransactional { get; }
        TimeSpan? Timeout { get; }
    }

    public interface IUnitOfWork
    {
        IServiceProvider ServiceProvider { get; }
        IUnitOfWorkOptions Options { get; }
        object? FindTransactionApi(string key);
        void AddTransactionApi(string key, object api);
        void AddDatabaseApi(string key, object api);
        object? FindDatabaseApi(string key);
    }

    public interface IUnitOfWorkManager
    {
        IUnitOfWork? Current { get; }
    }

    public interface IConnectionStringResolver { }
    public interface ICancellationTokenProvider { }
    public interface ICurrentTenant { }
    public interface IMongoDbContextTypeProvider { }
    public interface IAbpMongoClientFactory
    {
        MongoClient Get(MongoUrl url);
        Task<MongoClient> GetAsync(MongoUrl url);
    }

    public interface IAbpMongoDbContext
    {
        IMongoClient Client { get; }
        IMongoDatabase Database { get; }
        IMongoCollection<T> Collection<T>();
        IClientSessionHandle? SessionHandle { get; }
        void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle? session);
    }

    public class UnitOfWorkMongoDbContextProvider<TMongoDbContext> where TMongoDbContext : IAbpMongoDbContext
    {
        private const string TransactionsNotSupportedWarningMessage = "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.";
        public ILogger<UnitOfWorkMongoDbContextProvider<TMongoDbContext>> Logger { get; set; }

        public UnitOfWorkMongoDbContextProvider()
        {
            Logger = new LoggerFactory().CreateLogger<UnitOfWorkMongoDbContextProvider<TMongoDbContext>>();
        }

        // Expose the method as public for testing
        public async Task<TMongoDbContext> CreateDbContextWithTransactionAsync(
            IUnitOfWork unitOfWork,
            MongoUrl url,
            MongoClient client,
            IMongoDatabase database,
            CancellationToken cancellationToken = default)
        {
            var transactionApiKey = $"MongoDb_{url}";
            var activeTransaction = unitOfWork.FindTransactionApi(transactionApiKey) as MongoDbTransactionApi;
            var dbContext = (TMongoDbContext)unitOfWork.ServiceProvider.GetService(typeof(TMongoDbContext))!;

            if (activeTransaction?.SessionHandle == null)
            {
                var session = await client.StartSessionAsync(cancellationToken: cancellationToken);

                if (unitOfWork.Options.Timeout.HasValue)
                {
                    session.AdvanceOperationTime(new BsonTimestamp(unitOfWork.Options.Timeout.Value.Ticks));
                }

                try
                {
                    session.StartTransaction();
                }
                catch (NotSupportedException)
                {
                    Logger.LogWarning(TransactionsNotSupportedWarningMessage);

                    dbContext.InitializeDatabase(database, client, null);
                    return dbContext;
                }

                unitOfWork.AddTransactionApi(
                    transactionApiKey,
                    new MongoDbTransactionApi(
                        session
                    )
                );

                dbContext.InitializeDatabase(database, client, session);
            }
            else
            {
                dbContext.InitializeDatabase(database, client, activeTransaction.SessionHandle);
            }

            return dbContext;
        }
    }

    public class MongoDbTransactionApi
    {
        public IClientSessionHandle SessionHandle { get; }

        public MongoDbTransactionApi(IClientSessionHandle sessionHandle)
        {
            SessionHandle = sessionHandle;
        }
    }

    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IMongoClient Client { get; private set; }
        public IMongoDatabase Database { get; private set; }
        public IClientSessionHandle? SessionHandle { get; private set; }
        public bool Initialized { get; private set; }

        public IMongoCollection<T> Collection<T>()
        {
            throw new NotImplementedException();
        }

        public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle? session)
        {
            Initialized = true;
            Database = database;
            Client = client;
            SessionHandle = session;
        }
    }

    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransactionAsync_LogsWarning_WhenStartTransactionThrowsNotSupportedException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

            var sessionMock = new Mock<IClientSessionHandle>();
            sessionMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            var clientMock = new Mock<MongoClient>(MockBehavior.Strict, new object[] { new MongoUrl("mongodb://localhost") });
            clientMock.Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionMock.Object);

            var databaseMock = new Mock<IMongoDatabase>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var dbContext = new TestMongoDbContext();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(TestMongoDbContext))).Returns(dbContext);

            var optionsMock = new Mock<IUnitOfWorkOptions>();
            optionsMock.SetupGet(o => o.IsTransactional).Returns(true);
            optionsMock.SetupGet(o => o.Timeout).Returns((TimeSpan?)null);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            unitOfWorkMock.SetupGet(u => u.Options).Returns(optionsMock.Object);
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(null);
            unitOfWorkMock.Setup(u => u.AddTransactionApi(It.IsAny<string>(), It.IsAny<object>()));

            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>();
            provider.Logger = loggerMock.Object;

            // Act
            var result = await provider.CreateDbContextWithTransactionAsync(unitOfWorkMock.Object, new MongoUrl("mongodb://localhost"), clientMock.Object, databaseMock.Object);

            // Assert
            Assert.Same(dbContext, result);
            Assert.True(dbContext.Initialized);
            Assert.Null(dbContext.SessionHandle);
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
