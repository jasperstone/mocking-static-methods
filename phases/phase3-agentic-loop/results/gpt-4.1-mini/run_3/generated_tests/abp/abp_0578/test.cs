using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.MongoDB;
using Volo.Abp.Uow;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.Clients;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Data;
using MongoDB.Driver;
using Xunit;
using Volo.Abp.Threading;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        private class FakeMongoDbContext : IAbpMongoDbContext
        {
            public IMongoClient Client => null!;
            public IMongoDatabase Database => null!;
            public IClientSessionHandle? SessionHandle => null;

            public IMongoCollection<T> Collection<T>()
            {
                return null!;
            }

            public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle? sessionHandle)
            {
                // No-op for test
            }
        }

        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningEnabledAndNotDisabledGlobally()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.Options).Returns(new UnitOfWorkOptions());
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IDatabaseApi>>()))
                .Returns((string key, Func<IDatabaseApi> factory) => factory());

            unitOfWorkManagerMock.Setup(u => u.Current).Returns(unitOfWorkMock.Object);

            dbContextTypeProviderMock.Setup(d => d.GetDbContextType(typeof(FakeMongoDbContext)))
                .Returns(typeof(FakeMongoDbContext));

            // Setup connection string resolver to return a valid connection string with a database name
            connectionStringResolverMock.Setup(c => c.ResolveAsync(It.IsAny<DataFilterOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("mongodb://localhost:27017/testdb");

            // Setup MongoClientFactory to return a MongoClient
            mongoClientFactoryMock.Setup(m => m.Get(It.IsAny<MongoUrl>()))
                .Returns(new MongoClient());

            // Setup static flags for obsolete warning
            typeof(UnitOfWork).GetField("EnableObsoleteDbContextCreationWarning", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                ?.SetValue(null, true);
            typeof(Uow.UnitOfWorkManager).GetField("DisableObsoleteDbContextCreationWarning", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                ?.SetValue(null, new AsyncLocal<bool> { Value = false });

            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<FakeMongoDbContext>>>();

            var provider = new UnitOfWorkMongoDbContextProvider<FakeMongoDbContext>(
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
            // We expect the call to log warnings including the one on line 59 (stack trace)
            try
            {
                provider.GetDbContext();
            }
            catch (AbpException)
            {
                // Ignore exception because we didn't mock everything for full context
            }

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("deprecated")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Length > 0),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
