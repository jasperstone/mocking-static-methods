using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.EntityFrameworkCore;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransaction_Should_LogWarning_When_ExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<SampleDbContext>>>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<DatabaseFacade>();
            var dbContextMock = new Mock<SampleDbContext>();
            var transactionMock = new Mock<IDbContextTransaction>();

            // Setup the DbContext to return a DatabaseFacade with a BeginTransaction that throws
            dbContextMock.Setup(c => c.Database).Returns(databaseMock.Object);
            databaseMock.Setup(db => db.BeginTransaction()).Throws<InvalidOperationException>();

            // Setup the service provider to return the mocked DbContext
            serviceProviderMock.Setup(sp => sp.GetRequiredService<SampleDbContext>())
                .Returns(dbContextMock.Object);

            // Setup the unitOfWork to return the mocked service provider
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            // Setup the unitOfWork to have no active transaction
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(null);
            // Setup the unitOfWork to add a transaction api
            unitOfWorkMock.Setup(u => u.AddTransactionApi(It.IsAny<string>(), It.IsAny<object>()));

            var provider = new UnitOfWorkDbContextProvider<SampleDbContext>(
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<IConnectionStringResolver>(),
                Mock.Of<ICancellationTokenProvider>(),
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IEfCoreDbContextTypeProvider>())
            {
                Logger = loggerMock.Object
            };

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                // Call the method under test
                await provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);
            });

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(msg => msg.Contains("Current database does not support transactions"))),
                Times.Once);
        }
    }

    // Sample DbContext for testing
    public class SampleDbContext : DbContext, IEfCoreDbContext
    {
        public void Initialize(AbpEfCoreDbContextInitializationContext context) { }
    }
}
