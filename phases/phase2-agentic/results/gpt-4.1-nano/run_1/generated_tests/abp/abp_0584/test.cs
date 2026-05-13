using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.MongoDB;
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
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMongoClient> _mongoClientMock;
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<TMockMongoDbContext> _dbContextMock;

        public class MockMongoDbContext : IAbpMongoDbContext
        {
            public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle session) { }
            public object ToAbpMongoDbContext() => this;
        }

        public UnitOfWorkMongoDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
            _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockMongoDbContext>>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mongoClientMock = new Mock<IMongoClient>();
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _dbContextMock = new Mock<MockMongoDbContext>();

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
            var unitOfWork = _unitOfWorkMock.Object;
            var mongoUrl = new MongoUrl("mongodb://localhost:27017/testdb");
            var client = _mongoClientMock.Object;
            var database = _mongoDatabaseMock.Object;
            var cancellationToken = CancellationToken.None;

            var sessionMock = new Mock<IClientSessionHandle>();
            var sessionHandle = sessionMock.Object;

            // Setup client.StartSessionAsync to return session
            _mongoClientMock.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionHandle);

            // Setup session.StartTransaction to throw NotSupportedException
            sessionMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            // Setup unitOfWork.FindTransactionApi to return null
            _unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>()))
                .Returns(null);

            // Setup unitOfWork.ServiceProvider.GetRequiredService<T> to return our dbContext
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<MockMongoDbContext>())
                .Returns(_dbContextMock.Object);
            _unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);

            // Setup CreateDbContextWithTransactionAsync to return a dummy context
            var provider = new UnitOfWorkMongoDbContextProvider<MockMongoDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object
            );
            provider.Logger = _loggerMock.Object;

            // Act
            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await provider.CreateDbContextWithTransactionAsync(
                    unitOfWork,
                    mongoUrl,
                    client,
                    database,
                    cancellationToken
                );
            });

            // Verify that LogWarning was called with the expected message
            _loggerMock.Verify(
                logger => logger.LogWarning(It.Is<string>(msg => msg.Contains("Current database does not support transactions"))),
                Times.Once
            );
        }
    }
}
