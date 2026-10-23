using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;
using Volo.Abp.Uow.MongoDB;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransactionAsync_TransactionsNotSupported_LogsWarning()
        {
            // Arrange
            var unitOfWork = new Mock<IUnitOfWork>();
            var mongoUrl = new MongoUrl("mongodb://localhost:27017");
            var client = new Mock<IMongoClient>();
            var database = new Mock<IMongoDatabase>();
            var logger = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MyDbContext>>>();
            var provider = new UnitOfWorkMongoDbContextProvider<MyDbContext>(Mock.Of<IUnitOfWorkManager>(), 
                Mock.Of<IConnectionStringResolver>(), 
                Mock.Of<ICancellationTokenProvider>(), 
                Mock.Of<ICurrentTenant>(), 
                Mock.Of<IMongoDbContextTypeProvider>(), 
                Mock.Of<IAbpMongoClientFactory>())
            {
                Logger = logger.Object
            };

            client.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>())).Throws(new NotSupportedException());

            // Act
            await provider.CreateDbContextWithTransactionAsync(unitOfWork.Object, mongoUrl, client.Object, database.Object);

            // Assert
            logger.Verify(l => l.LogWarning(It.Is<string>(s => s == UnitOfWorkMongoDbContextProvider<MyDbContext>.TransactionsNotSupportedWarningMessage)), Times.Once);
        }

        private class MyDbContext : IAbpMongoDbContext
        {
            public IMongoDatabase Database { get; private set; }
            public IClientSessionHandle SessionHandle { get; private set; }
            public IMongoClient Client { get; private set; }

            public IMongoCollection<T> Collection<T>() where T : class
            {
                return Database.GetCollection<T>(typeof(T).Name);
            }

            public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle session = null)
            {
                Database = database;
                Client = client;
                SessionHandle = session;
            }
        }
    }
}
