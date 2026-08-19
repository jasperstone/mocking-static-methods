using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.UnitOfWork;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public async Task GetDbContext_Should_LogWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var currentUnitOfWorkMock = new Mock<IUnitOfWork>();
            var databaseApiMock = new Mock<IDatabaseApi>();
            var efCoreDbContextMock = new Mock<IEfCoreDbContext>();
            var efCoreTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<DbContext>>>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();

            var provider = new UnitOfWorkDbContextProvider<DbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            // Setup
            unitOfWorkManagerMock.Setup(m => m.Current).Returns(currentUnitOfWorkMock.Object);
            currentUnitOfWorkMock.Setup(m => m.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IDatabaseApi>>()))
                .Returns(databaseApiMock.Object);
            currentUnitOfWorkMock.Setup(m => m.Options).Returns(new UnitOfWorkOptions { IsTransactional = true });
            currentUnitOfWorkMock.Setup(m => m.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
            efCoreTypeProviderMock.Setup(m => m.GetDbContextType(It.IsAny<Type>())).Returns(typeof(DbContext));
            // Simulate environment where the static Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value is false
            // For this, we need to mock or set the static property, but since it's static, we assume it's false or not set.

            // Act
            await provider.GetDbContextAsync();

            // Assert
            // Verify that LogWarning was called with the expected message
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
