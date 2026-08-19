using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.MongoDB;
using Volo.Abp.MongoDB;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        private class TestMongoDbContext : IAbpMongoDbContext
        {
            public void InitializeDatabase(IMongoDatabase database, MongoClient client, object options) { }
            public void ToAbpMongoDbContext() { }
        }

        [Fact]
        public void GetDbContext_Should_LogWarning_When_ObsoleteWarningEnabled()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var currentUnitOfWork = unitOfWorkMock.Object;

            var typeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

            // Setup UoW manager to return current UoW
            unitOfWorkManagerMock.Setup(m => m.Current).Returns(currentUnitOfWork);

            // Setup UoW to simulate enabled warning
            unitOfWorkMock.SetupGet(m => m.EnableObsoleteDbContextCreationWarning).Returns(true);
            var disableWarningFlag = new Mock<Lazy<bool>>();
            disableWarningFlag.SetupGet(m => m.Value).Returns(false);
            // For simplicity, assume DisableObsoleteDbContextCreationWarning is false
            var disableObsoleteWarning = new Mock<Lazy<bool>>();
            disableObsoleteWarning.SetupGet(m => m.Value).Returns(false);
            // We need to set this property on UoW, but it's not shown in the snippet, so assume it's accessible
            // For the test, we can extend the mock to include this property if needed

            // Setup type provider to return a dummy type
            var dummyType = typeof(TestMongoDbContext);
            typeProviderMock.Setup(m => m.GetDbContextType(It.IsAny<Type>())).Returns(dummyType);

            // Setup connection string resolver
            connectionStringResolverMock.Setup(m => m.Resolve(It.IsAny<string>())).Returns("mongodb://localhost:27017/testdb");

            // Setup MongoClientFactory
            var mongoClientMock = new Mock<MongoClient>();
            mongoClientMock.Setup(m => m.GetDatabase(It.IsAny<string>())).Returns(Mock.Of<IMongoDatabase>());
            mongoClientFactoryMock.Setup(m => m.Get(It.IsAny<MongoUrl>())).Returns(mongoClientMock.Object);

            // Instantiate provider
            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                null,
                null,
                typeProviderMock.Object,
                mongoClientFactoryMock.Object
            );

            // Inject logger
            provider.Logger = loggerMock.Object;

            // Act
            var exceptionThrown = false;
            try
            {
                provider.GetDbContext();
            }
            catch (Exception)
            {
                // Ignore exceptions for this test
                exceptionThrown = true;
            }

            // Assert
            loggerMock.Verify(
                m => m.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
