using System;
using System.Data;
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
        [Fact]
        public void CreateDbContextWithTransaction_ShouldLogWarning_WhenExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TestUnitOfWorkDbContextProvider<TestDbContext>>>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var dbContextMock = new Mock<TestDbContext>();
            var dbContextTransactionMock = new Mock<IDbContextTransaction>();

            unitOfWorkManagerMock.Setup(u => u.Current).Returns(unitOfWorkMock.Object);
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            unitOfWorkMock.Setup(u => u.Options).Returns(new AbpUnitOfWorkOptions { IsolationLevel = IsolationLevel.ReadCommitted });
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((EfCoreTransactionApi)null);
            dbContextMock.Setup(d => d.Database.BeginTransaction(It.IsAny<IsolationLevel>())).Throws<InvalidOperationException>();

            var provider = new TestUnitOfWorkDbContextProvider<TestDbContext>(
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
            var result = provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Current database does not support transactions")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.NotNull(result);
        }

        public class TestDbContext : DbContext, IEfCoreDbContext
        {
            public DbContext AsDbContext => this;

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(0);
            }

            public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(0);
            }

            public void Initialize(AbpEfCoreDbContextInitializationContext context)
            {
                // Initialization logic
            }
        }

        public class TestUnitOfWorkDbContextProvider<TDbContext> : UnitOfWorkDbContextProvider<TDbContext>
            where TDbContext : IEfCoreDbContext
        {
            public TestUnitOfWorkDbContextProvider(
                IUnitOfWorkManager unitOfWorkManager,
                IConnectionStringResolver connectionStringResolver,
                ICancellationTokenProvider cancellationTokenProvider,
                ICurrentTenant currentTenant,
                IEfCoreDbContextTypeProvider efCoreDbContextTypeProvider)
                : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, efCoreDbContextTypeProvider)
            {
            }

            public new TDbContext CreateDbContextWithTransaction(IUnitOfWork unitOfWork)
            {
                return base.CreateDbContextWithTransaction(unitOfWork);
            }
        }
    }
}
