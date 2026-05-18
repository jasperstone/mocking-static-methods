using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        public void GetDbContext_ShouldLogWarning_WhenObsoleteDbContextCreationWarningIsEnabled()
        {
            // Arrange
            var unitOfWork = new Mock<IUnitOfWork>();
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(unitOfWork.Object);
            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object
            );
            provider.Logger = _loggerMock.Object;

            // Act
            provider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void GetDbContext_ShouldThrowAbpException_WhenNoUnitOfWork()
        {
            // Arrange
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns((IUnitOfWork)null);
            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object
            );

            // Act & Assert
            Assert.Throws<AbpException>(() => provider.GetDbContext());
        }

        [Fact]
        public async Task GetDbContextAsync_ShouldThrowAbpException_WhenNoUnitOfWork()
        {
            // Arrange
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns((IUnitOfWork)null);
            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object
            );

            // Act & Assert
            await Assert.ThrowsAsync<AbpException>(() => provider.GetDbContextAsync(CancellationToken.None));
        }
    }

    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IMongoClient Client { get; private set; }
        public IMongoDatabase Database { get; private set; }
        public IClientSessionHandle? SessionHandle { get; private set; }

        public IMongoCollection<T> Collection<T>()
        {
            return Database.GetCollection<T>(typeof(T).Name);
        }

        public Task<IClientSessionHandle> GetSessionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SessionHandle);
        }

        public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle session)
        {
            Database = database;
            Client = client;
            SessionHandle = session;
        }

        public void Dispose()
        {
            // Test implementation
        }
    }
}
