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
            public IMongoClient Client { get; private set; }
            public IMongoDatabase Database { get; private set; }
            public IClientSessionHandle? SessionHandle { get; private set; }
            public bool Initialized { get; private set; }

            public TestMongoDbContext()
            {
                // For simplicity, use nulls or mocks if needed
                Client = null!;
                Database = null!;
                SessionHandle = null;
                Initialized = false;
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
        public async Task CreateDbContextWithTransactionAsync_LogsWarning_WhenStartTransactionThrowsNotSupportedException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dbContext = new TestMongoDbContext();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(TestMongoDbContext))).Returns(dbContext);
            unitOfWorkMock.Setup(uow => uow.ServiceProvider).Returns(serviceProviderMock.Object);
            unitOfWorkMock.Setup(uow => uow.Options).Returns(new UnitOfWorkOptions { Timeout = null, IsTransactional = true });
            unitOfWorkMock.Setup(uow => uow.FindTransactionApi(It.IsAny<string>())).Returns(null);

            var sessionMock = new Mock<IClientSessionHandle>();
            sessionMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            var clientMock = new Mock<MongoClient>(MockBehavior.Strict, new object[] { new MongoUrl("mongodb://localhost") });
            clientMock.Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionMock.Object);

            var databaseMock = new Mock<IMongoDatabase>();

            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<IConnectionStringResolver>(),
                Mock.Of<ICancellationTokenProvider>(),
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IMongoDbContextTypeProvider>(),
                Mock.Of<IAbpMongoClientFactory>());

            provider.Logger = loggerMock.Object;

            // Act
            var result = await provider.CreateDbContextWithTransactionAsync(
                unitOfWorkMock.Object,
                new MongoUrl("mongodb://localhost"),
                clientMock.Object,
                databaseMock.Object);

            // Assert
            Assert.Same(dbContext, result);
            Assert.True(dbContext.Initialized);
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
