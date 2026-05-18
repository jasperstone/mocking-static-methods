using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.MongoDB;

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
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;

        public UnitOfWorkMongoDbContextProviderTests()
        {
            _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockDbContext>>>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _serviceProviderMock = new Mock<IServiceProvider>();
        }

        [Fact]
        public async Task LogWarning_IsCalled_When_CreateDbContextWithTransaction_Throws_NotSupportedException()
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

            var mockClient = new Mock<MongoClient>();
            var mockDatabase = new Mock<IMongoDatabase>();
            var mockSession = new Mock<IClientSessionHandle>();
            var mockDbContext = new Mock<MockDbContext>();
            var transactionApi = new MongoDbTransactionApi(mockSession.Object, null);

            // Setup
            _unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(transactionApi);
            _unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<MockDbContext>()).Returns(mockDbContext.Object);
            mockClient.Setup(c => c.StartSession()).Returns(mockSession.Object);
            mockSession.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            // Act
            var result = await provider.CreateDbContextWithTransaction(_unitOfWorkMock.Object, new MongoUrl("mongodb://test"), mockClient.Object, mockDatabase.Object);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("Current database does not support transactions"))),
                Times.Once);
        }
    }

    // Dummy DbContext for testing
    public class MockDbContext : IAbpMongoDbContext
    {
        public void ToAbpMongoDbContext() { }
        public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle session) { }
    }
}
