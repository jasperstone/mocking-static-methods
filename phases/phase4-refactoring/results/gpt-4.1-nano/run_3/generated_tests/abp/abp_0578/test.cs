using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.MongoDB;
using Volo.Abp.MongoDB;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.UnitOfWork;

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
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<UnitOfWorkMongoDbContextProvider<SampleMongoDbContext>>> _loggerMock;
        private readonly UnitOfWorkMongoDbContextProvider<SampleMongoDbContext> _provider;

        public class SampleMongoDbContext : IAbpMongoDbContext
        {
            public void InitializeDatabase(IMongoDatabase database, MongoClient client, object options) { }
            public void ToAbpMongoDbContext() { }
        }

        public UnitOfWorkMongoDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<SampleMongoDbContext>>>();

            _provider = new UnitOfWorkMongoDbContextProvider<SampleMongoDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object
            );

            // Inject the mock logger
            _provider.Logger = _loggerMock.Object;
        }

        [Fact]
        public void GetDbContext_ShouldLogWarning_WhenCalledOutsideUnitOfWork()
        {
            // Arrange
            _unitOfWorkManagerMock.Setup(m => m.Current).Returns((IUnitOfWork)null);

            // Act & Assert
            var exception = Assert.Throws<AbpException>(() => _provider.GetDbContext());
            Assert.Contains("A", exception.Message); // Confirm exception message contains "A"

            // Verify that LogWarning was called with the expected message
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("deprecated") || v.ToString().Contains("LINQ")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Specifically check that the message contains the environment stack trace
            _loggerMock.Verify(
                logger => logger.LogWarning(It.Is<string>(msg => msg.Contains(Environment.StackTrace.Substring(0, Math.Min(50, Environment.StackTrace.Length))))),
                Times.AtLeastOnce);
        }
    }
}
