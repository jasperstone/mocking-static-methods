using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
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
            var mongoUrl = new MongoUrl("mongodb://localhost:27017");
            var client = new Mock<MongoClient>();
            var database = new Mock<IMongoDatabase>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var logger = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();
            var mongoClientFactory = new Mock<IAbpMongoClientFactory>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var currentTenant = new Mock<ICurrentTenant>();
            var dbContextTypeProvider = new Mock<IMongoDbContextTypeProvider>();

            unitOfWork.Setup(u => u.Options.Timeout).Returns((int?)null);
            unitOfWork.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((MongoDbTransactionApi)null);
            unitOfWork.Setup(u => u.ServiceProvider.GetRequiredService<TestMongoDbContext>()).Returns(new TestMongoDbContext());
            client.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>())).Throws(new NotSupportedException());
            mongoClientFactory.Setup(f => f.GetAsync(mongoUrl)).ReturnsAsync(client.Object);

            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                new Mock<IUnitOfWorkManager>().Object,
                new Mock<IConnectionStringResolver>().Object,
                cancellationTokenProvider.Object,
                currentTenant.Object,
                dbContextTypeProvider.Object,
                mongoClientFactory.Object
            );
            provider.Logger = logger.Object;

            // Act
            await provider.CreateDbContextWithTransactionAsync(unitOfWork.Object, mongoUrl, client.Object, database.Object);

            // Assert
            logger.Verify(l => l.LogWarning(It.Is<string>(s => s == "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.")), Times.Once);
        }
    }

    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IMongoClient Client => new MongoClient("mongodb://localhost:27017");

        public IMongoDatabase Database => Client.GetDatabase("test");

        public IMongoCollection<T> Collection<T>()
        {
            return Database.GetCollection<T>(typeof(T).Name);
        }

        public IClientSessionHandle? SessionHandle => null;
    }
}
