using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransactionAsync_LogsWarning_WhenTransactionsAreNotSupported()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mongoClientMock = new Mock<MongoClient>();
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MyMongoDbContext>>>();
            var provider = new UnitOfWorkMongoDbContextProvider<MyMongoDbContext>(
                unitOfWorkMock.Object,
                null,
                null,
                null,
                null,
                null
            );
            provider.Logger = loggerMock.Object;

            unitOfWorkMock.Setup(u => u.Options.IsTransactional).Returns(true);
            mongoClientMock.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>())).Throws(new NotSupportedException());

            // Act
            await provider.CreateDbContextWithTransactionAsync(
                unitOfWorkMock.Object,
                new MongoUrl("mongodb://localhost:27017"),
                mongoClientMock.Object,
                mongoClientMock.Object.GetDatabase("mydb")
            );

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }

        private class MyMongoDbContext : IAbpMongoDbContext
        {
            public IMongoCollection<T> Collection<T>(string name) where T : class
            {
                throw new NotImplementedException();
            }

            public IMongoClient Client => throw new NotImplementedException();

            public IMongoDatabase Database => throw new NotImplementedException();

            public IClientSessionHandle SessionHandle => null;

            public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle session)
            {
                // No-op
            }

            public IAbpMongoDbContext ToAbpMongoDbContext()
            {
                return this;
            }
        }
    }
}
