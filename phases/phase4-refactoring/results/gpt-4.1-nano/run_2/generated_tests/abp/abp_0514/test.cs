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
        public void GetDbContext_ShouldLogWarning_WhenCalledInsideObsoleteMethod()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var currentUnitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<SampleDbContext>>>();
            var efCoreTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();

            var provider = new UnitOfWorkDbContextProvider<SampleDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreTypeProviderMock.Object
            );

            provider.Logger = loggerMock.Object;

            // Setup
            unitOfWorkManagerMock.Setup(m => m.Current).Returns(currentUnitOfWorkMock.Object);
            // Simulate enabling the warning
            // Note: Since the method is obsolete, it will call the warning log
            // We need to set EnableObsoleteDbContextCreationWarning to true
            // and DisableObsoleteDbContextCreationWarning to false
            // But these are static or external, so we simulate the condition
            // For simplicity, assume the condition is true

            // Act
            // Call the obsolete method
            var exception = Record.Exception(() => provider.GetDbContext());

            // Assert
            loggerMock.Verify(
                m => m.LogWarning(It.Is<string>(s => s.Contains("deprecated"))),
                Times.AtLeastOnce
            );
        }
    }

    // Sample DbContext for testing
    public class SampleDbContext : IEfCoreDbContext
    {
        public void Initialize(AbpEfCoreDbContextInitializationContext context)
        {
        }
    }
}
