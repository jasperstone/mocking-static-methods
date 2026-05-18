using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private const string TransactionsNotSupportedWarningMessage = "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.";

        [Fact]
        public void CreateDbContextWithTransaction_ShouldLogWarning_WhenTransactionNotSupported()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var dbContextMock = new Mock<TestDbContext>();

            unitOfWorkManagerMock.Setup(u => u.Current).Returns(unitOfWorkMock.Object);
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((EfCoreTransactionApi)null);
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            unitOfWorkMock.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsolationLevel = IsolationLevel.ReadCommitted });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<TestDbContext>()).Returns(dbContextMock.Object);
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);

            dbContextMock.Setup(d => d.Database.BeginTransaction(It.IsAny<IsolationLevel>())).Throws<InvalidOperationException>();

            var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            // Act
            provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(TransactionsNotSupportedWarningMessage)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        public class TestDbContext : DbContext, IEfCoreDbContext
        {
            public DbContext AsDbContext => this;

            public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                return SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }

            public void Initialize(AbpEfCoreDbContextInitializationContext context)
            {
                // Initialization logic
            }
        }
    }
}
