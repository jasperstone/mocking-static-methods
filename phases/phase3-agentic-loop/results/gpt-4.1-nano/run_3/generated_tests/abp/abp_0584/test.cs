using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.MongoDB;
using Volo.Abp.MongoDB;
using Volo.Abp.DependencyInjection;
using Volo.Abp;
using MongoDB.Driver;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IMongoDbContextTypeProvider> _dbContextTypeProviderMock;
        private readonly Mock<IAbpMongoClientFactory> _mongoClientFactoryMock;
        private readonly Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockMongoDbContext>>> _loggerMock;
        private readonly UnitOfWorkMongoDbContextProvider<MockMongoDbContext> _provider;

        public UnitOfWorkMongoDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
            _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockMongoDbContext>>>();

            _provider = new UnitOfWorkMongoDbContextProvider<MockMongoDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object
            );
            _provider.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task CreateDbContextWithTransactionAsync_Should_LogWarning_When_NotSupportedExceptionThrown()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mongoUrl = new MongoUrl("mongodb://localhost:27017/testdb");
            var clientMock = new Mock<IMongoClient>();
            var databaseMock = new Mock<IMongoDatabase>();
            var sessionMock = new Mock<IClientSessionHandle>();
            var database = new Mock<IMongoDatabase>();
            var optionsMock = new Mock<IUnitOfWorkOptions>();
            var transactionApiMock = new Mock<MongoDbTransactionApi>();
            var sessionHandle = (IClientSessionHandle)null;

            // Setup unitOfWork to return null for FindTransactionApi
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(null);
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
            unitOfWorkMock.Setup(u => u.Options).Returns(new UnitOfWorkOptions { Timeout = TimeSpan.FromSeconds(30), IsTransactional = true });
            unitOfWorkMock.Setup(u => u.AddTransactionApi(It.IsAny<string>(), It.IsAny<MongoDbTransactionApi>()));

            // Setup MongoClientFactory to return clientMock
            _mongoClientFactoryMock.Setup(f => f.GetAsync(It.IsAny<MongoUrl>())).ReturnsAsync(clientMock.Object);

            // Setup clientMock to return sessionMock
            clientMock.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sessionMock.Object);

            // Setup sessionMock to throw NotSupportedException on StartTransaction
            sessionMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            // Act
            var result = await _provider.CreateDbContextWithTransactionAsync(
                unitOfWorkMock.Object,
                mongoUrl,
                clientMock.Object,
                databaseMock.Object,
                CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(msg => msg.Contains("Current database does not support transactions"))),
                Times.Once);
        }
    }

    // Dummy context class for testing
    public class MockMongoDbContext : IAbpMongoDbContext
    {
        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle session)
        {
            // no-op
        }

        public object ToAbpMongoDbContext() => this;
    }
}
