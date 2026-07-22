using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.Clients;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.MongoDB;
using Xunit;
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
        private readonly Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>> _loggerMock;

        public UnitOfWorkMongoDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
            _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();
        }

        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteDbContextCreationWarningIsEnabled()
        {
            // Arrange
            var unitOfWorkMongoDbContextProvider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object
            );

            unitOfWorkMongoDbContextProvider.Logger = _loggerMock.Object;

            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(new Mock<IUnitOfWork>().Object);
            _dbContextTypeProviderMock.Setup(d => d.GetDbContextType(typeof(TestMongoDbContext))).Returns(typeof(TestMongoDbContext));
            _connectionStringResolverMock.Setup(c => c.Resolve(It.IsAny<string>())).Returns("mongodb://localhost:27017");

            // Act
            unitOfWorkMongoDbContextProvider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task GetDbContextAsync_ReturnsDbContext_WhenUnitOfWorkIsAvailable()
        {
            // Arrange
            var unitOfWorkMongoDbContextProvider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object
            );

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(unitOfWorkMock.Object);
            _dbContextTypeProviderMock.Setup(d => d.GetDbContextType(typeof(TestMongoDbContext))).Returns(typeof(TestMongoDbContext));
            _connectionStringResolverMock.Setup(c => c.ResolveAsync(It.IsAny<string>())).ReturnsAsync("mongodb://localhost:27017");

            // Act
            var result = await unitOfWorkMongoDbContextProvider.GetDbContextAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }
    }

    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IMongoDatabase Database { get; private set; }
        public IMongoClient Client { get; private set; }
        public IClientSessionHandle SessionHandle { get; private set; }

        public IMongoCollection<T> Collection<T>()
        {
            return Database.GetCollection<T>(typeof(T).Name);
        }

        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle session)
        {
            Database = database;
            Client = client;
            SessionHandle = session;
        }

        public Task InitializeDatabaseAsync(IMongoDatabase database, IMongoClient client, IClientSessionHandle session, CancellationToken cancellationToken = default)
        {
            InitializeDatabase(database, client, session);
            return Task.CompletedTask;
        }

        public Task EnsureIndexesCreatedAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnsureIndexesCreatedAsync(IClientSessionHandle session, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnsureIndexesCreatedAsync<T>(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnsureIndexesCreatedAsync<T>(IClientSessionHandle session, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnsureIndexesCreatedAsync<T>(IMongoCollection<T> collection, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnsureIndexesCreatedAsync<T>(IMongoCollection<T> collection, IClientSessionHandle session, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnsureIndexesCreatedAsync<T>(IEnumerable<CreateIndexModel<T>> indexes, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnsureIndexesCreatedAsync<T>(IEnumerable<CreateIndexModel<T>> indexes, IClientSessionHandle session, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnsureIndexesCreatedAsync<T>(IMongoCollection<T> collection, IEnumerable<CreateIndexModel<T>> indexes, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task EnsureIndexesCreatedAsync<T>(IMongoCollection<T> collection, IEnumerable<CreateIndexModel<T>> indexes, IClientSessionHandle session, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
