using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockMongoContext>>> _loggerMock;
        private readonly ServiceCollection _services;

        public UnitOfWorkMongoDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
            _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockMongoContext>>>();
            _services = new ServiceCollection();
        }

        [Fact]
        public async Task CreateDbContextWithTransactionAsync_Should_LogWarning_When_NotSupportedExceptionThrown()
        {
            // Arrange
            var provider = new UnitOfWorkMongoDbContextProvider<MockMongoContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object
            );
            provider.Logger = _loggerMock.Object;

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockSessionHandle = new object();
            var mockTransactionApi = new MongoDbTransactionApi
            {
                SessionHandle = null
            };
            mockUnitOfWork.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(mockTransactionApi);
            mockUnitOfWork.Setup(u => u.ServiceProvider).Returns(_services.BuildServiceProvider());

            var mockClient = new Mock<IMongoClient>();
            var mockDatabase = new Mock<IMongoDatabase>();
            var mockSession = new Mock<IClientSessionHandle>();
            mockClient.Setup(c => c.StartSession()).Returns(mockSession.Object);
            mockSession.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            _mongoClientFactoryMock.Setup(f => f.Get(It.IsAny<MongoUrl>())).Returns(mockClient.Object);

            var mongoUrl = new MongoUrl("mongodb://localhost:27017");
            var database = mockClient.Object.GetDatabase("testdb");

            // Act
            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await provider.CreateDbContextWithTransactionAsync(
                    mockUnitOfWork.Object,
                    mongoUrl,
                    mockClient.Object,
                    database,
                    CancellationToken.None);
            });

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("Current database does not support transactions"))),
                Times.Once);
        }
    }

    // Dummy context class for testing
    public class MockMongoContext : IAbpMongoDbContext
    {
        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle session) { }
        public MockMongoContext ToAbpMongoDbContext() => this;
    }
}
