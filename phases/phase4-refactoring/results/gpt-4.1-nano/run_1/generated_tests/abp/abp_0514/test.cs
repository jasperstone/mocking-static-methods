using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.Uow.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace Volo.Abp.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public async Task GetDbContextAsync_ShouldLogWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<SampleDbContext>>>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var databaseApiMock = new Mock<IEfCoreDatabaseApi>();
            var dbContextMock = new Mock<SampleDbContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup dependencies
            var provider = new UnitOfWorkDbContextProvider<SampleDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object
            );
            provider.Logger = loggerMock.Object;

            // Setup UnitOfWorkManager.Current to return mock
            unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);

            // Setup static properties and methods
            // Enable warning
            // Note: Since static properties are used, we need to set them directly
            // For the purpose of this test, assume EnableObsoleteDbContextCreationWarning is true
            // and DisableObsoleteDbContextCreationWarning.Value is false
            // These are static or global, so in real tests, you'd need to set them accordingly
            // Here, we simulate that condition

            // Setup UnitOfWork to return current
            unitOfWorkMock.SetupGet(u => u).Returns(unitOfWorkMock.Object);
            // Setup UnitOfWork to have a current value
            // For simplicity, assume UnitOfWorkManager.Current is set to unitOfWorkMock.Object
            // and that EnableObsoleteDbContextCreationWarning is true
            // and DisableObsoleteDbContextCreationWarning.Value is false

            // Setup GetOrAddDatabaseApi to return mock databaseApi
            unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IEfCoreDatabaseApi>>()))
                .Returns(databaseApiMock.Object);

            // Setup EfCoreDbContextTypeProvider to return a type
            efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(typeof(SampleDbContext)))
                .Returns(typeof(SampleDbContext));

            // Setup ConnectionStringNameAttribute to return a string
            // For simplicity, assume it returns "Default"
            // Since it's static, we can't mock it directly, so we assume the code uses "Default"

            // Setup ResolveConnectionString to return a string
            connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<string>()))
                .Returns("Server=.;Database=TestDb;Trusted_Connection=True;");

            // Act
            await provider.GetDbContextAsync();

            // Assert
            // Verify that LogWarning was called with a message containing the specific warning text
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }

    // Sample DbContext for testing
    public class SampleDbContext : DbContext, IEfCoreDbContext
    {
        public void Initialize(AbpEfCoreDbContextInitializationContext context)
        {
        }
    }
}
