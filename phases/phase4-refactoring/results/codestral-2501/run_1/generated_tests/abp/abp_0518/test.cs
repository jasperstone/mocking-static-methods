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
        [Fact]
        public void CreateDbContextWithTransaction_ShouldLogWarning_WhenExceptionIsThrown()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            var loggerMock = new Mock<ILogger<TestUnitOfWorkDbContextProvider>>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkManagerMock.Setup(u => u.Current).Returns(unitOfWorkMock.Object);

            var dbContextMock = new Mock<TestDbContext>();
            dbContextMock.Setup(d => d.Database.BeginTransaction(It.IsAny<IsolationLevel>())).Throws<InvalidOperationException>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<TestDbContext>()).Returns(dbContextMock.Object);
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);

            var provider = new TestUnitOfWorkDbContextProvider(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object
            );
            provider.Logger = loggerMock.Object;

            // Act
            provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }

        public class TestUnitOfWorkDbContextProvider : UnitOfWorkDbContextProvider<TestDbContext>
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

            public new TestDbContext CreateDbContextWithTransaction(IUnitOfWork unitOfWork)
            {
                return base.CreateDbContextWithTransaction(unitOfWork);
            }
        }

        public class TestDbContext : DbContext, IEfCoreDbContext
        {
            public void Initialize(AbpEfCoreDbContextInitializationContext context)
            {
                // Implementation not needed for the test
            }

            public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                // Implementation not needed for the test
                return Task.FromResult(0);
            }
        }
    }
}
