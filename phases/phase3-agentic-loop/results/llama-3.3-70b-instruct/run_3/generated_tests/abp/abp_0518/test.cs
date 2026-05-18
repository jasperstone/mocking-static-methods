using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Uow.EntityFrameworkCore;
using System;
using Volo.Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public void CreateDbContextWithTransaction_LogsWarning_WhenTransactionsAreNotSupported()
        {
            // Arrange
            var unitOfWorkMock = new Mock<Volo.Abp.Uow.IUnitOfWork>();
            var serviceProviderMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceProvider>();
            var dbContextMock = new Mock<IEfCoreDbContext>();
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<IEfCoreDbContext>>>();

            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IEfCoreDbContext>()).Returns(dbContextMock.Object);
            var dbContext = new Mock<DbContext>();
            dbContext.Setup(d => d.BeginTransaction(It.IsAny<IsolationLevel>())).Throws(new NotSupportedException());
            dbContextMock.Setup(d => d.Database).Returns(dbContext.Object);

            var provider = new UnitOfWorkDbContextProvider<IEfCoreDbContext>(
                Mock.Of<Volo.Abp.Uow.IUnitOfWorkManager>(),
                Mock.Of<Volo.Abp.Data.IConnectionStringResolver>(),
                Mock.Of<Volo.Abp.Threading.ICancellationTokenProvider>(),
                Mock.Of<Volo.Abp.MultiTenancy.ICurrentTenant>(),
                Mock.Of<Volo.Abp.EntityFrameworkCore.IEfCoreDbContextTypeProvider>()
            );
            provider.Logger = loggerMock.Object;

            // Act
            provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s == "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.")), Times.Once);
        }
    }
}
