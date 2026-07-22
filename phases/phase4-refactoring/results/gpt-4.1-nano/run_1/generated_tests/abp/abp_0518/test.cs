using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.EntityFrameworkCore;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransaction_Should_LogWarning_OnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<SampleDbContext>>>();
            var provider = new UnitOfWorkDbContextProvider<SampleDbContext>(
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<IConnectionStringResolver>(),
                Mock.Of<ICancellationTokenProvider>(),
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IEfCoreDbContextTypeProvider>()
            );
            provider.Logger = loggerMock.Object;

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var dbContextMock = new Mock<SampleDbContext>();
            var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            var transactionMock = new Mock<IDbContextTransaction>();

            // Setup the dbContext to return a DatabaseFacade with a transaction
            dbContextMock.Setup(c => c.Database).Returns(databaseMock.Object);
            databaseMock.Setup(db => db.BeginTransaction(It.IsAny<IsolationLevel>())).Throws<InvalidOperationException>();

            // Setup the service provider to return the dbContext
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<SampleDbContext>()).Returns(dbContextMock.Object);
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            unitOfWorkMock.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = true, IsolationLevel = null });
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(null);
            unitOfWorkMock.Setup(u => u.AddTransactionApi(It.IsAny<string>(), It.IsAny<object>()));

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                provider.CreateDbContextWithTransaction(unitOfWorkMock.Object));

            // Assert
            loggerMock.Verify(
                log => log.LogWarning(It.Is<string>(s => s.Contains("Current database does not support transactions"))),
                Times.Once);
        }
    }

    // Sample DbContext for testing
    public class SampleDbContext : DbContext, IEfCoreDbContext
    {
        public void Initialize(AbpEfCoreDbContextInitializationContext context) { }
    }
}
