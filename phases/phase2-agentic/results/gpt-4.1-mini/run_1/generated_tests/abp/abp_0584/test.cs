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

        [Fact]
        public async Task CreateDbContextWithTransactionAsync_LogsWarning_WhenStartTransactionThrowsNotSupportedException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

            var sessionMock = new Mock<IClientSessionHandle>();
            sessionMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            var clientMock = new Mock<MongoClient>();
            clientMock.Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionMock.Object);

            var databaseMock = new Mock<IMongoDatabase>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var dbContext = new TestMongoDbContext();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(TestMongoDbContext))).Returns(dbContext);

            var transactionApiKey = "MongoDb_mongodb://localhost:27017";
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(uow => uow.FindTransactionApi(transactionApiKey)).Returns((object)null);
            unitOfWorkMock.Setup(uow => uow.ServiceProvider).Returns(serviceProviderMock.Object);
            unitOfWorkMock.Setup(uow => uow.Options).Returns(new UnitOfWorkOptions { Timeout = null });

            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            cancellationTokenProviderMock.Setup(c => c.GetCancellationToken(It.IsAny<CancellationToken>())).Returns(CancellationToken.None);

            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            dbContextTypeProviderMock.Setup(d => d.GetDbContextType(typeof(TestMongoDbContext))).Returns(typeof(TestMongoDbContext));

            var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
            mongoClientFactoryMock.Setup(f => f.GetAsync(It.IsAny<MongoUrl>(), It.IsAny<CancellationToken>())).ReturnsAsync(clientMock.Object);

            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                Mock.Of<IUnitOfWorkManager>(m => m.Current == unitOfWorkMock.Object),
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                dbContextTypeProviderMock.Object,
                mongoClientFactoryMock.Object
            );
            provider.Logger = loggerMock.Object;

            // Act
            var result = await provider.CreateDbContextWithTransactionAsync(
                unitOfWorkMock.Object,
                new MongoUrl("mongodb://localhost:27017"),
                clientMock.Object,
                databaseMock.Object,
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
            Assert.Same(databaseMock.Object, dbContext.Database);
            Assert.Same(clientMock.Object, dbContext.Client);
        }
    }
}
