using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Uow.MongoDB
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningIsEnabled()
        {
            // Arrange
            var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var connectionStringResolver = new Mock<IConnectionStringResolver>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var currentTenant = new Mock<ICurrentTenant>();
            var dbContextTypeProvider = new Mock<IMongoDbContextTypeProvider>();
            var logger = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                unitOfWorkManager.Object,
                connectionStringResolver.Object,
                cancellationTokenProvider.Object,
                currentTenant.Object,
                dbContextTypeProvider.Object,
                new Mock<IAbpMongoClientFactory>().Object
            );

            provider.Logger = logger.Object;

            unitOfWorkManager.Setup(u => u.Current).Returns(new Mock<IUnitOfWork>().Object);

            // Act
            provider.GetDbContext();

            // Assert
            logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetDbContext_ThrowsAbpException_WhenUnitOfWorkIsNotStarted()
        {
            // Arrange
            var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var connectionStringResolver = new Mock<IConnectionStringResolver>();
            var cancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var currentTenant = new Mock<ICurrentTenant>();
            var dbContextTypeProvider = new Mock<IMongoDbContextTypeProvider>();

            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                unitOfWorkManager.Object,
                connectionStringResolver.Object,
                cancellationTokenProvider.Object,
                currentTenant.Object,
                dbContextTypeProvider.Object,
                new Mock<IAbpMongoClientFactory>().Object
            );

            unitOfWorkManager.Setup(u => u.Current).Returns((IUnitOfWork)null);

            // Act and Assert
            Assert.Throws<AbpException>(() => provider.GetDbContext());
        }

        private class TestMongoDbContext : IAbpMongoDbContext
        {
            public IMongoClient Client { get; set; }

            public IMongoDatabase Database { get; set; }

            public IMongoCollection<T> Collection<T>()
            {
                throw new NotImplementedException();
            }

            public IClientSessionHandle? SessionHandle { get; set; }

            public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle? session)
            {
                Database = database;
                Client = client;
                SessionHandle = session;
            }
        }
    }
}
