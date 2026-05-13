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
        public void GetDbContext_LogsWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<DummyMongoDbContext>>>();

            // Setup UnitOfWork static properties to enable warning
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
            Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

            // Setup UnitOfWorkManager.Current to a dummy unit of work that throws on GetOrAddDatabaseApi to avoid full execution
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<object>>()))
                .Returns(new MongoDbDatabaseApi(new DummyMongoDbContext()));

            unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);

            // Setup DbContextTypeProvider to return DummyMongoDbContext type
            dbContextTypeProviderMock.Setup(m => m.GetDbContextType(typeof(DummyMongoDbContext)))
                .Returns(typeof(DummyMongoDbContext));

            // Setup ConnectionStringResolver to return a valid connection string with database name
            connectionStringResolverMock.Setup(m => m.ResolveAsync(It.IsAny<Type>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("mongodb://localhost:27017/testdb");

            connectionStringResolverMock.Setup(m => m.Resolve(It.IsAny<Type>()))
                .Returns("mongodb://localhost:27017/testdb");

            // Setup MongoClientFactory to return a mock MongoClient
            var mongoClientMock = new Mock<MongoDB.Driver.MongoClient>(MockBehavior.Strict, new object[] { "mongodb://localhost:27017" });
            mongoClientFactoryMock.Setup(m => m.Get(It.IsAny<MongoDB.Driver.MongoUrl>())).Returns(mongoClientMock.Object);

            // Setup MongoClient.GetDatabase to return a mock IMongoDatabase
            var mongoDatabaseMock = new Mock<MongoDB.Driver.IMongoDatabase>();
            mongoClientMock.Setup(c => c.GetDatabase("testdb", null)).Returns(mongoDatabaseMock.Object);

            // Setup unitOfWork.ServiceProvider.GetRequiredService to return DummyMongoDbContext instance
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(DummyMongoDbContext))).Returns(new DummyMongoDbContext());
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);

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
