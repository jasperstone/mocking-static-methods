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
            var databaseContextMock = new Mock<MockMongoDbContext>();
            var transactionApi = new MongoDbTransactionApi(sessionMock.Object, null);

            // Setup unitOfWork to return null for FindTransactionApi
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(null);
            // Setup unitOfWork to return a service provider that returns the dbContext
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<MockMongoDbContext>()).Returns(databaseContextMock.Object);
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            // Setup unitOfWork to add transaction api
            unitOfWorkMock.Setup(u => u.AddTransactionApi(It.IsAny<string>(), It.IsAny<object>()));

            // Setup client.StartSession to throw NotSupportedException
            clientMock.Setup(c => c.StartSession()).Returns(sessionMock.Object);
            sessionMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            // Setup logger to verify LogWarning call
            var loggerMock = new Mock<ILogger>();
            _provider.Logger = loggerMock.Object;

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

            // Verify that LogWarning was called with the specific message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Current database does not support transactions")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Mock implementation of TMongoDbContext for testing
    public class MockMongoDbContext : IAbpMongoDbContext
    {
        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle session)
        {
            // No-op for testing
        }

        public IAbpMongoDbContext ToAbpMongoDbContext() => this;
    }
}
