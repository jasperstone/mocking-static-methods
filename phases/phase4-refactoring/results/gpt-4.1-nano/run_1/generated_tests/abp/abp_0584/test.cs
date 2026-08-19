using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Volo.Abp.Uow.MongoDB;

namespace TestNamespace
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransaction_ShouldLogWarning_WhenNotSupportedExceptionThrown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockDbContext>>>();
            var provider = new UnitOfWorkMongoDbContextProvider<MockDbContext>(
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<IConnectionStringResolver>(),
                Mock.Of<ICancellationTokenProvider>(),
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IMongoDbContextTypeProvider>(),
                Mock.Of<IAbpMongoClientFactory>()
            );
            provider.Logger = mockLogger.Object;

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockClient = new Mock<MongoClient>();
            var mockDatabase = new Mock<IMongoDatabase>();
            var mockSession = new Mock<IClientSessionHandle>();
            var mockDbContext = new Mock<MockDbContext>();

            // Setup the client to return a session
            mockClient.Setup(c => c.StartSession()).Returns(mockSession.Object);
            // Setup the session to throw NotSupportedException
            mockSession.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();

            // Setup the database context to initialize database
            mockDbContext.Setup(d => d.ToAbpMongoDbContext()).Returns(new Mock<IAbpMongoDbContext>().Object);

            // Act
            await provider.CreateDbContextWithTransaction(
                mockUnitOfWork.Object,
                new MongoUrl("mongodb://test"),
                mockClient.Object,
                mockDatabase.Object
            );

            // Assert
            mockLogger.Verify(
                log => log.LogWarning(It.Is<string>(s => s.Contains("Current database does not support transactions"))),
                Times.Once
            );
        }
    }

    public class MockDbContext : IAbpMongoDbContext
    {
        public void InitializeDatabase(IMongoDatabase database, MongoClient client, IClientSessionHandle session)
        {
            // Implementation not needed for test
        }
    }
}
