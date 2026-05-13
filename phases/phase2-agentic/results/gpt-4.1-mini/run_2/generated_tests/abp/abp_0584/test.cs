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

            public new async Task<TestMongoDbContext> CreateDbContextWithTransactionAsync(
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
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var uowOptions = new UnitOfWorkOptions { IsTransactional = true };
            var uowOptionsMock = new Mock<IUnitOfWorkOptions>();
            uowOptionsMock.SetupGet(o => o.IsTransactional).Returns(true);
            uowOptionsMock.SetupGet(o => o.Timeout).Returns((TimeSpan?)null);

            unitOfWorkMock.SetupGet(u => u.Options).Returns(uowOptions);
            unitOfWorkMock.SetupGet(u => u.ServiceProvider).Returns(serviceProviderMock.Object);

            var mongoUrl = new MongoUrl("mongodb://localhost:27017/testdb");
            var clientMock = new Mock<MongoClient>(MockBehavior.Strict, new object[] { mongoUrl });
            var databaseMock = new Mock<IMongoDatabase>();

            var sessionMock = new Mock<IClientSessionHandle>();
            sessionMock.Setup(s => s.AdvanceOperationTime(It.IsAny<BsonTimestamp>()));
            sessionMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            clientMock.Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionMock.Object);
            clientMock.Setup(c => c.StartSession(It.IsAny<ClientSessionOptions>()))
                .Returns(sessionMock.Object);
            clientMock.Setup(c => c.GetDatabase(It.IsAny<string>(), null))
                .Returns(databaseMock.Object);

            var dbContext = new TestMongoDbContext();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(TestMongoDbContext))).Returns(dbContext);

            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(null);
            unitOfWorkMock.Setup(u => u.AddTransactionApi(It.IsAny<string>(), It.IsAny<object>()));

            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

            mongoClientFactoryMock.Setup(f => f.GetAsync(It.IsAny<MongoUrl>())).ReturnsAsync(clientMock.Object);

            var provider = new TestMongoDbContextProvider(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                dbContextTypeProviderMock.Object,
                mongoClientFactoryMock.Object);

            provider.Logger = loggerMock.Object;

            // Act
            var result = await provider.CreateDbContextWithTransactionAsync(
                unitOfWorkMock.Object,
                mongoUrl,
                clientMock.Object,
                databaseMock.Object,
                CancellationToken.None);

            // Assert
            Assert.Same(dbContext, result);
            Assert.True(dbContext.Initialized);
            Assert.Null(dbContext.Session);
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
