using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.Clients;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        private readonly Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>> _loggerMock;
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IMongoDbContextTypeProvider> _dbContextTypeProviderMock;
        private readonly Mock<IAbpMongoClientFactory> _mongoClientFactoryMock;
        private readonly UnitOfWorkMongoDbContextProvider<TestMongoDbContext> _provider;

        public UnitOfWorkMongoDbContextProviderTests()
        {
            _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

            _provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task CreateDbContextWithTransactionAsync_ShouldLogWarning_WhenTransactionsNotSupported()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mongoUrl = new MongoUrl("mongodb://localhost:27017");
            var mongoClientMock = new Mock<MongoClient>();
            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            var dbContextMock = new Mock<TestMongoDbContext>();

            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((MongoDbTransactionApi)null);
            unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<TestMongoDbContext>()).Returns(dbContextMock.Object);
            mongoClientMock.Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotSupportedException());

            // Act
            await _provider.CreateDbContextWithTransactionAsync(unitOfWorkMock.Object, mongoUrl, mongoClientMock.Object, mongoDatabaseMock.Object);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Current database does not support transactions")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }

    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IMongoClient Client => throw new NotImplementedException();

        public IMongoDatabase Database => throw new NotImplementedException();

        public IMongoCollection<T> Collection<T>() => throw new NotImplementedException();

        public IClientSessionHandle? SessionHandle => throw new NotImplementedException();

        public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle session)
        {
            // Mock implementation
        }
    }
}
