using System;
using System.Threading;
using System.Threading.Tasks;
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
            var clientMock = new Mock<MongoClient>();
            var databaseMock = new Mock<IMongoDatabase>();
            var sessionMock = new Mock<IClientSessionHandle>();
            var dbContextMock = new Mock<MockMongoDbContext>();
            var transactionApi = new MongoDbTransactionApi(sessionMock.Object, null);

            // Setup unitOfWork to return null for FindTransactionApi
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(null);
            // Setup unitOfWork to return the mocked service provider
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<MockMongoDbContext>()).Returns(dbContextMock.Object);
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            // Setup unitOfWork to add transaction api
            unitOfWorkMock.Setup(u => u.AddTransactionApi(It.IsAny<string>(), It.IsAny<MongoDbTransactionApi>()));

            // Setup MongoClient to return a session
            var startSessionCalled = false;
            clientMock.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionMock.Object)
                .Callback(() => startSessionCalled = true);
            // Setup session to throw NotSupportedException on StartTransaction
            sessionMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            // Setup MongoClientFactory to return our mock client
            _mongoClientFactoryMock.Setup(f => f.Get(It.IsAny<MongoUrl>())).Returns(clientMock.Object);

            // Act
            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await _provider.CreateDbContextWithTransactionAsync(
                    unitOfWorkMock.Object,
                    mongoUrl,
                    clientMock.Object,
                    databaseMock.Object,
                    CancellationToken.None);
            });

            // Verify that LogWarning was called with the expected message
            _loggerMock.Verify(
                logger => logger.LogWarning(It.Is<string>(s => s.Contains("Current database does not support transactions"))),
                Times.Once);
        }
    }

    // Mock implementations for context
    public class MockMongoDbContext : IAbpMongoDbContext
    {
        public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle session)
        {
            // No-op for mock
        }

        public IAbpMongoDbContext ToAbpMongoDbContext() => this;
    }
}
