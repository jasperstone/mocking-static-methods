using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransaction_LogsWarning_WhenTransactionsAreNotSupported()
        {
            // Arrange
            var unitOfWork = new Mock<IUnitOfWork>();
            var dbContext = new Mock<IEfCoreDbContext>();
            var logger = new Mock<ILogger<UnitOfWorkDbContextProvider<IEfCoreDbContext>>>();
            var provider = new UnitOfWorkDbContextProvider<IEfCoreDbContext>(Mock.Of<IUnitOfWorkManager>(), 
                Mock.Of<IConnectionStringResolver>(), 
                Mock.Of<ICancellationTokenProvider>(), 
                Mock.Of<ICurrentTenant>(), 
                Mock.Of<IEfCoreDbContextTypeProvider>());

            provider.Logger = logger.Object;

            unitOfWork.Setup(u => u.Options.IsTransactional).Returns(true);
            unitOfWork.Setup(u => u.ServiceProvider.GetRequiredService<IEfCoreDbContext>()).Returns(dbContext.Object);
            dbContext.Setup(d => d.Database.BeginTransaction(It.IsAny<IsolationLevel>())).Throws(new NotSupportedException());

            // Act
            provider.CreateDbContextWithTransaction(unitOfWork.Object);

            // Assert
            logger.Verify(l => l.LogWarning(It.Is<string>(s => s == "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.")), Times.Once);
        }
    }
}
