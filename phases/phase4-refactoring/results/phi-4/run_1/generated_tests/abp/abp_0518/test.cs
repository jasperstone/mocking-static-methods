using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Uow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace AbpTests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private readonly Mock<ILogger<UnitOfWorkDbContextProvider<DbContext>>> _loggerMock;
        private readonly UnitOfWorkDbContextProvider<DbContext> _provider;

        public UnitOfWorkDbContextProviderTests()
        {
            _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<DbContext>>>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

            _provider = new UnitOfWorkDbContextProvider<DbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                efCoreDbContextTypeProviderMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void CreateDbContextWithTransaction_LogsWarning_WhenTransactionNotSupported()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.Options.IsolationLevel).Returns((IsolationLevel?)null);
            unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<DbContext>()).Returns(new DbContext());

            // Act
            _provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Current database does not support transactions. Your database may remain in an inconsistent state in an error case."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }
    }
}
