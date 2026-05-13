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
        private readonly UnitOfWorkMongoDbContextProvider<TestMongoDbContext> _provider;

        public UnitOfWorkMongoDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
            _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

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
        public void GetDbContext_ShouldLogWarning_WhenObsoleteDbContextCreationWarningIsEnabled()
        {
            // Arrange
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(new Mock<IUnitOfWork>().Object);
            _unitOfWorkManagerMock.Setup(u => u.DisableObsoleteDbContextCreationWarning).Returns(false);
            _dbContextTypeProviderMock.Setup(d => d.GetDbContextType(typeof(TestMongoDbContext))).Returns(typeof(TestMongoDbContext));
            _connectionStringResolverMock.Setup(c => c.ResolveConnectionString(It.IsAny<Type>())).Returns("mongodb://localhost:27017/testdb");

            // Act
            var result = _provider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                l => l.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task GetDbContextAsync_ShouldReturnDbContext_WhenUnitOfWorkIsAvailable()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(unitOfWorkMock.Object);
            _dbContextTypeProviderMock.Setup(d => d.GetDbContextType(typeof(TestMongoDbContext))).Returns(typeof(TestMongoDbContext));
            _connectionStringResolverMock.Setup(c => c.ResolveConnectionStringAsync(It.IsAny<Type>(), It.IsAny<CancellationToken>())).ReturnsAsync("mongodb://localhost:27017/testdb");
            var mongoClientMock = new Mock<MongoClient>();
            _mongoClientFactoryMock.Setup(m => m.GetAsync(It.IsAny<MongoUrl>(), It.IsAny<CancellationToken>())).ReturnsAsync(mongoClientMock.Object);
            var dbContextMock = new Mock<TestMongoDbContext>();
            unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<TestMongoDbContext>()).Returns(dbContextMock.Object);

            // Act
            var result = await _provider.GetDbContextAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dbContextMock.Object, result);
        }
    }

    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle session)
        {
            // Test implementation
        }
    }
}
