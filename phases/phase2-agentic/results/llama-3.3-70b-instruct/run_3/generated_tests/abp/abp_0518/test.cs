using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransaction_LogsWarning_WhenTransactionIsNotSupported()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<MyDbContext>>>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dbContextMock = new Mock<MyDbContext>();

            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<MyDbContext>()).Returns(dbContextMock.Object);
            dbContextMock.Setup(d => d.Database.BeginTransaction(It.IsAny<IsolationLevel>())).Throws(new NotSupportedException());

            var provider = new UnitOfWorkDbContextProvider<MyDbContext>(unitOfWorkMock.Object, loggerMock.Object);

            // Act
            provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s == UnitOfWorkDbContextProvider<MyDbContext>.TransactionsNotSupportedWarningMessage)), Times.Once);
        }
    }

    public class MyDbContext : DbContext, IEfCoreDbContext
    {
        public DbContext DbContext => this;
    }
}
