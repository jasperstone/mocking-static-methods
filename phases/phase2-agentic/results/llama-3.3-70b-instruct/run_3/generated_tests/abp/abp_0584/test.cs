using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using System;
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
            var client = new Mock<MongoClient>();
            var database = new Mock<IMongoDatabase>();
            var dbContext = new Mock<IAbpMongoDbContext>();
            var logger = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<IAbpMongoDbContext>>>();
            var provider = new UnitOfWorkMongoDbContextProvider<IAbpMongoDbContext>(
                new Mock<IUnitOfWorkManager>().Object,
                new Mock<IConnectionStringResolver>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new Mock<ICurrentTenant>().Object,
                new Mock<IMongoDbContextTypeProvider>().Object,
                new Mock<IAbpMongoClientFactory>().Object
            )
            {
                Logger = logger.Object
            };

            client.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>()))
                .Throws(new NotSupportedException());

            // Act
            await provider.CreateDbContextWithTransactionAsync(
                unitOfWork.Object,
                mongoUrl,
                client.Object,
                database.Object,
                CancellationToken.None
            );

            // Assert
            logger.Verify(l => l.LogWarning(It.Is<string>(s => s == UnitOfWorkMongoDbContextProvider<IAbpMongoDbContext>.TransactionsNotSupportedWarningMessage)), Times.Once);
        }
    }
}
