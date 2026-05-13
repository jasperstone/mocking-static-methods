using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.MongoDB;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        private class DummyMongoDbContext : IAbpMongoDbContext
        {
            public void InitializeDatabase(MongoDB.Driver.IMongoDatabase database, MongoDB.Driver.MongoClient client, object session)
            {
                // No-op for test
            }
        }

        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteDbContextCreationWarningEnabled()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

            // Setup UnitOfWorkManager.Current to a dummy unit of work that throws on GetOrAddDatabaseApi to avoid full flow
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<object>>()))
                .Returns(new MongoDbDatabaseApi(new DummyMongoDbContext()));

            unitOfWorkManagerMock.Setup(u => u.Current).Returns(unitOfWorkMock.Object);

            // Setup DbContextTypeProvider to return DummyMongoDbContext type
            dbContextTypeProviderMock.Setup(d => d.GetDbContextType(typeof(DummyMongoDbContext)))
                .Returns(typeof(DummyMongoDbContext));

            // Setup ConnectionStringResolver to return a valid connection string with database name
            connectionStringResolverMock.Setup(c => c.ResolveAsync(It.IsAny<Type>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("mongodb://localhost:27017/testdb");
            connectionStringResolverMock.Setup(c => c.Resolve(It.IsAny<Type>()))
                .Returns("mongodb://localhost:27017/testdb");

            // Setup static flags for warning
            typeof(UnitOfWork).GetField("EnableObsoleteDbContextCreationWarning", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                ?.SetValue(null, true);
            typeof(Uow.UnitOfWorkManager).GetField("DisableObsoleteDbContextCreationWarning", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                ?.SetValue(null, new Lazy<bool>(() => false));

            // Create a mock logger to verify LogWarning calls
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<DummyMongoDbContext>>>();

            var provider = new UnitOfWorkMongoDbContextProvider<DummyMongoDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                dbContextTypeProviderMock.Object,
                mongoClientFactoryMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            // Act
            var context = provider.GetDbContext();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("at ")), // StackTrace contains "at "
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
