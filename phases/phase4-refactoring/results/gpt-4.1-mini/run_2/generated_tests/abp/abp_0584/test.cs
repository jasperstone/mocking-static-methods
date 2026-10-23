using System;
using System.Threading;
using System.Threading.Tasks;
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
        private class FakeMongoDbContext : IAbpMongoDbContext
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

        private class FakeUnitOfWork : IUnitOfWork
        {
            public IServiceProvider ServiceProvider { get; }
            public IUnitOfWorkOptions Options { get; }
            private readonly object _transactionApi;
            private readonly string _transactionApiKey;
            private readonly IServiceProvider _serviceProvider;

            public FakeUnitOfWork(IServiceProvider serviceProvider, IUnitOfWorkOptions options, string transactionApiKey = null, object transactionApi = null)
            {
                _serviceProvider = serviceProvider;
                ServiceProvider = serviceProvider;
                Options = options;
                _transactionApiKey = transactionApiKey;
                _transactionApi = transactionApi;
            }

            public object FindTransactionApi(string key)
            {
                if (key == _transactionApiKey)
                {
                    return _transactionApi;
                }
                return null;
            }

            public void AddTransactionApi(string key, object api)
            {
                // no-op for test
            }

            public object FindDatabaseApi(string key) => null;
            public void AddDatabaseApi(string key, object api) { }
        }

        private class FakeUnitOfWorkOptions : IUnitOfWorkOptions
        {
            public bool IsTransactional { get; set; }
            public TimeSpan? Timeout { get; set; }
        }

        [Fact]
        public async Task CreateDbContextWithTransactionAsync_LogsWarning_WhenStartTransactionThrowsNotSupportedException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<FakeMongoDbContext>>>();

            var sessionMock = new Mock<IClientSessionHandle>();
            sessionMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            var clientMock = new Mock<MongoClient>(MockBehavior.Strict, new object[] { new MongoClientSettings() });
            clientMock.Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionMock.Object);

            var databaseMock = new Mock<IMongoDatabase>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var fakeDbContext = new FakeMongoDbContext();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(FakeMongoDbContext))).Returns(fakeDbContext);

            var unitOfWorkOptions = new FakeUnitOfWorkOptions { IsTransactional = true };
            var unitOfWork = new FakeUnitOfWork(serviceProviderMock.Object, unitOfWorkOptions);

            var provider = new UnitOfWorkMongoDbContextProvider<FakeMongoDbContext>(
                unitOfWorkManager: null,
                connectionStringResolver: null,
                cancellationTokenProvider: null,
                currentTenant: null,
                dbContextTypeProvider: new FakeDbContextTypeProvider(typeof(FakeMongoDbContext)),
                mongoClientFactory: new FakeMongoClientFactory(clientMock.Object)
            );
            provider.Logger = loggerMock.Object;

            // Act
            var result = await InvokeCreateDbContextWithTransactionAsync(provider, unitOfWork, new MongoUrl("mongodb://localhost/testdb"), clientMock.Object, databaseMock.Object);

            // Assert
            Assert.Same(fakeDbContext, result);
            Assert.True(fakeDbContext.Initialized);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Current database does not support transactions")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private static async Task<FakeMongoDbContext> InvokeCreateDbContextWithTransactionAsync(
            UnitOfWorkMongoDbContextProvider<FakeMongoDbContext> provider,
            IUnitOfWork unitOfWork,
            MongoUrl url,
            MongoClient client,
            IMongoDatabase database)
        {
            // Use reflection to call protected method CreateDbContextWithTransactionAsync
            var method = typeof(UnitOfWorkMongoDbContextProvider<FakeMongoDbContext>).GetMethod("CreateDbContextWithTransactionAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<FakeMongoDbContext>)method.Invoke(provider, new object[] { unitOfWork, url, client, database, CancellationToken.None });
            return await task;
        }

        private class FakeDbContextTypeProvider : IMongoDbContextTypeProvider
        {
            private readonly Type _type;
            public FakeDbContextTypeProvider(Type type)
            {
                _type = type;
            }
            public Type GetDbContextType(Type dbContextType)
            {
                return _type;
            }
        }

        private class FakeMongoClientFactory : IAbpMongoClientFactory
        {
            private readonly MongoClient _client;
            public FakeMongoClientFactory(MongoClient client)
            {
                _client = client;
            }
            public MongoClient Get(MongoUrl url) => _client;
            public Task<MongoClient> GetAsync(MongoUrl url) => Task.FromResult(_client);
        }
    }
}
