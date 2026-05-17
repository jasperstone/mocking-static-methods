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
using Volo.Abp.Uow.MongoDB;
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
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<MongoClient> _mongoClientMock;
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IClientSessionHandle> _clientSessionHandleMock;
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
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mongoClientMock = new Mock<MongoClient>();
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _clientSessionHandleMock = new Mock<IClientSessionHandle>();

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
            var mongoUrl = new MongoUrl("mongodb://localhost:27017");
            var transactionApiKey = $"MongoDb_{mongoUrl}";
            _unitOfWorkMock.Setup(uow => uow.FindTransactionApi(transactionApiKey)).Returns((MongoDbTransactionApi)null);
            _unitOfWorkMock.Setup(uow => uow.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
            _mongoClientFactoryMock.Setup(factory => factory.GetAsync(mongoUrl)).ReturnsAsync(_mongoClientMock.Object);
            _mongoClientMock.Setup(client => client.GetDatabase(It.IsAny<string>(), null)).Returns(_mongoDatabaseMock.Object);
            _mongoClientMock.Setup(client => client.StartSessionAsync(It.IsAny<ClientSessionOptions>(), default)).ReturnsAsync(_clientSessionHandleMock.Object);
            _clientSessionHandleMock.Setup(session => session.StartTransaction(It.IsAny<TransactionOptions>())).Throws<NotSupportedException>();

            // Act
            await _provider.CreateDbContextWithTransactionAsync(_unitOfWorkMock.Object, mongoUrl, _mongoClientMock.Object, _mongoDatabaseMock.Object);

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Once);
        }
    }

    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IMongoCollection<T> Collection<T>()
        {
            throw new NotImplementedException();
        }

        public IMongoClient Client { get; set; }

        public IMongoDatabase Database { get; set; }

        public IClientSessionHandle? SessionHandle { get; set; }

        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle? sessionHandle)
        {
            // Mock implementation
        }
    }
}
