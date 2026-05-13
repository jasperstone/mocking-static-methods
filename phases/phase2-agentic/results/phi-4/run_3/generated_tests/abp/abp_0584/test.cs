using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.Clients;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        private readonly Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockDbContext>>> _loggerMock;
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IMongoDbContextTypeProvider> _dbContextTypeProviderMock;
        private readonly Mock<IAbpMongoClientFactory> _mongoClientFactoryMock;

        public UnitOfWorkMongoDbContextProviderTests()
        {
            _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockDbContext>>>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
        }

        [Fact]
        public async Task CreateDbContextWithTransactionAsync_LogsWarning_WhenNotSupportedExceptionIsThrown()
        {
            // Arrange
            var provider = new UnitOfWorkMongoDbContextProvider<MockDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object)
            {
                Logger = _loggerMock.Object
            };

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mongoUrl = new MongoUrl("mongodb://localhost:27017");
            var clientMock = new Mock<MongoClient>();
            var databaseMock = new Mock<IMongoDatabase>();

            _unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);
            _mongoClientFactoryMock.Setup(m => m.Get(mongoUrl)).Returns(clientMock.Object);
            clientMock.Setup(m => m.GetDatabase(It.IsAny<string>())).Returns(databaseMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() =>
                provider.CreateDbContextWithTransactionAsync(
                    unitOfWorkMock.Object,
                    mongoUrl,
                    clientMock.Object,
                    databaseMock.Object,
                    CancellationToken.None));

            _loggerMock.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(s => s.Contains("Current database does not support transactions")),
                    It.IsAny<Exception>()),
                Times.Once);
        }
    }

    public class MockDbContext : IAbpMongoDbContext
    {
        public void InitializeDatabase(IMongoDatabase database, MongoClient client, IMongoClientSession session)
        {
            // Mock implementation
        }

        public IAbpMongoDbContext ToAbpMongoDbContext()
        {
            return this;
        }
    }
}
