using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Uow.MongoDB
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransactionAsync_LogsWarning_WhenTransactionsAreNotSupported()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MyDbContext>>>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mongoUrl = new MongoUrl("mongodb://localhost:27017");
            var clientMock = new Mock<IMongoClient>();
            var databaseMock = new Mock<IMongoDatabase>();
            var sessionMock = new Mock<IClientSessionHandle>();
            clientMock.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sessionMock.Object);
            sessionMock.Setup(s => s.StartTransaction()).Throws(new NotSupportedException());
            var dbContextProvider = new UnitOfWorkMongoDbContextProvider<MyDbContext>(unitOfWorkMock.Object, null, null, null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            await dbContextProvider.CreateDbContextWithTransactionAsync(unitOfWorkMock.Object, mongoUrl, clientMock.Object, databaseMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }

        private class MyDbContext : IAbpMongoDbContext
        {
            public IMongoCollection<BsonDocument> Collection<T>() => null;
            public IMongoClient Client => null;
            public IMongoDatabase Database => null;
            public IClientSessionHandle SessionHandle => null;
            public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle session)
            {
                // No-op
            }
        }
    }
}
