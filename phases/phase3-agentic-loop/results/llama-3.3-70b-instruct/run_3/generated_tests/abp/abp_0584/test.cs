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

namespace Volo.Abp.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransactionAsync_LogsWarning_WhenTransactionsAreNotSupported()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mongoClientMock = new Mock<IMongoClient>();
            var mongoUrl = new MongoUrl("mongodb://localhost:27017");
            var databaseName = "testdatabase";

            unitOfWorkMock.Setup(u => u.Options.IsTransactional).Returns(true);
            unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<TestMongoDbContext>()).Returns(new TestMongoDbContext());
            mongoClientMock.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>())).Throws(new NotSupportedException());

            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                unitOfWorkMock.Object,
                new Mock<IConnectionStringResolver>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new Mock<ICurrentTenant>().Object,
                new Mock<IMongoDbContextTypeProvider>().Object,
                new Mock<IAbpMongoClientFactory>().Object
            );
            provider.Logger = loggerMock.Object;

            // Act
            await provider.CreateDbContextWithTransactionAsync(unitOfWorkMock.Object, mongoUrl, mongoClientMock.Object, mongoClientMock.Object.GetDatabase(databaseName));

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s == UnitOfWorkMongoDbContextProvider<TestMongoDbContext>.TransactionsNotSupportedWarningMessage)), Times.Once);
        }
    }

    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IMongoCollection<T> Collection<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public IMongoClient Client => throw new NotImplementedException();

        public IMongoDatabase Database => throw new NotImplementedException();

        public IClientSessionHandle SessionHandle => throw new NotImplementedException();

        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle session)
        {
        }
    }
}
