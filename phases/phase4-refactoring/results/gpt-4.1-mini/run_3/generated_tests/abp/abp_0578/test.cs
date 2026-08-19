using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow.MongoDB;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    // Minimal fake implementation of IAbpMongoDbContext for testing
    public class FakeMongoDbContext : IAbpMongoDbContext
    {
        public IMongoClient Client => null!;
        public IMongoDatabase Database => null!;
        public IMongoCollection<T> Collection<T>() => null!;
        public IClientSessionHandle? SessionHandle => null;
    }

    // Minimal stubs for missing interfaces and classes
    public interface IUnitOfWorkManager
    {
        IUnitOfWork? Current { get; }
    }

    public interface IUnitOfWork
    {
        T GetOrAddDatabaseApi<T>(string key, Func<T> factory) where T : class;
        UnitOfWorkOptions Options { get; }
        IServiceProvider ServiceProvider { get; }
    }

    public class UnitOfWorkOptions
    {
        public bool IsTransactional { get; set; }
    }

    public interface IConnectionStringResolver { }
    public interface ICancellationTokenProvider { }
    public interface ICurrentTenant { }
    public interface IMongoDbContextTypeProvider
    {
        Type GetDbContextType(Type dbContextType);
    }
    public interface IAbpMongoClientFactory { }

    public class MongoDbDatabaseApi
    {
        public object? DbContext { get; }
        public MongoDbDatabaseApi(object? dbContext) { DbContext = dbContext; }
    }

    // Static flags simulation
    public static class UnitOfWork
    {
        public static bool EnableObsoleteDbContextCreationWarning { get; set; } = false;
    }

    public static class Uow
    {
        public static class UnitOfWorkManager
        {
            public static Lazy<bool> DisableObsoleteDbContextCreationWarning { get; set; } = new Lazy<bool>(() => false);
        }
    }

    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteDbContextCreationWarningEnabled()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<MongoDbDatabaseApi>>()))
                .Returns(new MongoDbDatabaseApi(new FakeMongoDbContext()));
            unitOfWorkMock.SetupGet(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = false });
            unitOfWorkMock.SetupGet(u => u.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);

            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            unitOfWorkManagerMock.SetupGet(m => m.Current).Returns(unitOfWorkMock.Object);

            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            dbContextTypeProviderMock.Setup(m => m.GetDbContextType(typeof(FakeMongoDbContext))).Returns(typeof(FakeMongoDbContext));
            var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<FakeMongoDbContext>>>();

            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
            Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning = new Lazy<bool>(() => false);

            var provider = new UnitOfWorkMongoDbContextProvider<FakeMongoDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                dbContextTypeProviderMock.Object,
                mongoClientFactoryMock.Object
            );

            provider.Logger = loggerMock.Object;

            // Act
            var ex = Record.Exception(() => provider.GetDbContext());

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("at ")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
