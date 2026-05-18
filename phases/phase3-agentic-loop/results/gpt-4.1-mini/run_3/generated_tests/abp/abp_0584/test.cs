using System;
using System.Threading;
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
            public IMongoClient Client { get; private set; }
            public IMongoDatabase Database { get; private set; }
            public IClientSessionHandle? SessionHandle { get; private set; }
            public bool Initialized { get; private set; }

            public TestMongoDbContext()
            {
            }

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

        [Fact]
        public void CreateDbContextWithTransaction_LogsWarning_WhenStartTransactionThrowsNotSupportedException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IUnitOfWorkOptions>();

            var mongoClientMock = new Mock<MongoClient>(MockBehavior.Strict, (MongoClientSettings)null);
            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            var sessionMock = new Mock<IClientSessionHandle>();

            // Setup unitOfWork to return null for FindTransactionApi to simulate no active transaction
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(null);

            // Setup unitOfWork options
            optionsMock.SetupGet(o => o.Timeout).Returns((TimeSpan?)null);
            optionsMock.SetupGet(o => o.IsTransactional).Returns(true);
            unitOfWorkMock.SetupGet(u => u.Options).Returns(optionsMock.Object);

            // Setup unitOfWork ServiceProvider to return a TestMongoDbContext instance
            var testDbContext = new TestMongoDbContext();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(TestMongoDbContext))).Returns(testDbContext);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<TestMongoDbContext>()).Returns(testDbContext);
            unitOfWorkMock.SetupGet(u => u.ServiceProvider).Returns(serviceProviderMock.Object);

            // Setup MongoClient to return session mock and throw on StartTransaction
            mongoClientMock.Setup(c => c.StartSession(It.IsAny<ClientSessionOptions>())).Returns(sessionMock.Object);
            sessionMock.Setup(s => s.AdvanceOperationTime(It.IsAny<BsonTimestamp>()));
            sessionMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            // Setup MongoClientFactory to return our mongoClientMock
            var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
            mongoClientFactoryMock.Setup(f => f.Get(It.IsAny<MongoUrl>())).Returns(mongoClientMock.Object);

            // Setup other dependencies with mocks or nulls as they are not used in this test
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            unitOfWorkManagerMock.SetupGet(m => m.Current).Returns(unitOfWorkMock.Object);

            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            dbContextTypeProviderMock.Setup(p => p.GetDbContextType(typeof(TestMongoDbContext))).Returns(typeof(TestMongoDbContext));

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
            var result = provider.CreateDbContextWithTransaction(
                unitOfWorkMock.Object,
                new MongoUrl("mongodb://localhost:27017/testdb"),
                mongoClientMock.Object,
                mongoDatabaseMock.Object
            );

            // Assert
            Assert.Same(testDbContext, result);
            Assert.True(testDbContext.Initialized);
            Assert.Null(testDbContext.SessionHandle);
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
