using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.EntityFrameworkCore;

namespace Volo.Abp.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransaction_Should_LogWarning_When_ExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<SampleDbContext>>>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IUnitOfWorkOptions>();
            var databaseMock = new Mock<IRelationalDatabaseFacadeDependencies>();
            var dbContextMock = new Mock<SampleDbContext>();
            var transactionMock = new Mock<IDbContextTransaction>();

            // Setup the unitOfWork to simulate no active transaction
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(null);
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            unitOfWorkMock.Setup(u => u.Options).Returns(optionsMock.Object);
            unitOfWorkMock.Setup(u => u.AddTransactionApi(It.IsAny<string>(), It.IsAny<object>()));

            // Setup the service provider to throw InvalidOperationException when GetRequiredService is called
            serviceProviderMock.Setup(sp => sp.GetRequiredService<SampleDbContext>())
                .Throws(new InvalidOperationException());

            // Create the provider instance
            var provider = new UnitOfWorkDbContextProvider<SampleDbContext>(
                unitOfWorkManagerMock.Object,
                null,
                null,
                null,
                null)
            {
                Logger = loggerMock.Object
            };

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                provider.CreateDbContextWithTransaction(unitOfWorkMock.Object));

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(msg => msg.Contains("Current database does not support transactions"))),
                Times.Once);
        }
    }

    // Sample DbContext for testing
    public class SampleDbContext : DbContext, IEfCoreDbContext
    {
        public void Initialize(AbpEfCoreDbContextInitializationContext context) { }
    }
}
