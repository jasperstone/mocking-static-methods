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
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<MongoClient> _mongoClientMock;
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IClientSessionHandle> _clientSessionHandleMock;
        private readonly Mock<TestMongoDbContext> _dbContextMock;

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
            _dbContextMock = new Mock<TestMongoDbContext>();
        }

        [Fact]
        public async Task CreateDbContextWithTransactionAsync_ShouldLogWarning_WhenTransactionsNotSupported()
        {
            // Arrange
            var provider = new TestableUnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object
            )
            {
                Logger = _loggerMock.Object
            };

            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(_unitOfWorkMock.Object);
            _unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            _unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<TestMongoDbContext>()).Returns(_dbContextMock.Object);
            _mongoClientFactoryMock.Setup(f => f.GetAsync(It.IsAny<MongoUrl>())).ReturnsAsync(_mongoClientMock.Object);
            _mongoClientMock.Setup(c => c.GetDatabase(It.IsAny<string>())).Returns(_mongoDatabaseMock.Object);
            _mongoClientMock.Setup(c => c.StartSession(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>())).Throws<NotSupportedException>();

            // Act
            await provider.CreateDbContextWithTransactionAsync(
                _unitOfWorkMock.Object,
                new MongoUrl("mongodb://localhost"),
                _mongoClientMock.Object,
                _mongoDatabaseMock.Object,
                CancellationToken.None
            );

            // Assert
            _loggerMock.Verify(
                x => x.Log(
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
        public IMongoCollection<T> Collection<T>()
        {
            return Mock.Of<IMongoCollection<T>>();
        }

        public IMongoClient Client => Mock.Of<IMongoClient>();

        public IMongoDatabase Database => Mock.Of<IMongoDatabase>();

        public IClientSessionHandle? SessionHandle => Mock.Of<IClientSessionHandle>();

        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle? session)
        {
            // Mock implementation
        }
    }

    public class TestableUnitOfWorkMongoDbContextProvider<TMongoDbContext> : UnitOfWorkMongoDbContextProvider<TMongoDbContext>
        where TMongoDbContext : IAbpMongoDbContext
    {
        public TestableUnitOfWorkMongoDbContextProvider(
            IUnitOfWorkManager unitOfWorkManager,
            IConnectionStringResolver connectionStringResolver,
            ICancellationTokenProvider cancellationTokenProvider,
            ICurrentTenant currentTenant,
            IMongoDbContextTypeProvider dbContextTypeProvider,
            IAbpMongoClientFactory mongoClientFactory)
            : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, dbContextTypeProvider, mongoClientFactory)
        {
        }

        public new virtual Task<TMongoDbContext> CreateDbContextWithTransactionAsync(
            IUnitOfWork unitOfWork,
            MongoUrl url,
            MongoClient client,
            IMongoDatabase database,
            CancellationToken cancellationToken = default)
        {
            return base.CreateDbContextWithTransactionAsync(unitOfWork, url, client, database, cancellationToken);
        }
    }
}
