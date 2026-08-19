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
    public class UnitOfWorkMongoDbContextProviderTests
    {
        private class TestMongoDbContext : IAbpMongoDbContext
        {
            public IMongoClient Client => throw new NotImplementedException();
            public IMongoDatabase Database => throw new NotImplementedException();
            public IMongoCollection<T> Collection<T>() => throw new NotImplementedException();
            public IClientSessionHandle? SessionHandle => null;
        }

        [Fact]
        public void GetDbContext_LogsWarningWithStackTrace_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<object>(); // Placeholder, no interface found
            var cancellationTokenProviderMock = new Mock<object>(); // Placeholder, no interface found
            var currentTenantMock = new Mock<object>(); // Placeholder, no interface found
            var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            var mongoClientFactoryMock = new Mock<object>(); // Placeholder, no interface found

            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

            // Setup UnitOfWorkManager.Current to a dummy unit of work to avoid exception
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.Options).Returns(new UnitOfWorkOptions());
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);

            // Setup DbContextTypeProvider to return TestMongoDbContext type
            dbContextTypeProviderMock.Setup(d => d.GetDbContextType(typeof(TestMongoDbContext)))
                .Returns(typeof(TestMongoDbContext));

            // Create provider instance
            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                unitOfWorkManagerMock.Object,
                (IConnectionStringResolver)connectionStringResolverMock.Object,
                (ICancellationTokenProvider)cancellationTokenProviderMock.Object,
                (ICurrentTenant)currentTenantMock.Object,
                dbContextTypeProviderMock.Object,
                (IAbpMongoClientFactory)mongoClientFactoryMock.Object
            );

            provider.Logger = loggerMock.Object;

            // Act
            try
            {
                provider.GetDbContext();
            }
            catch
            {
                // Ignore exceptions from incomplete mocks
            }

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
                    It.Is<It.IsAnyType>((v, t) => !string.IsNullOrEmpty(v.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }
    }
}
