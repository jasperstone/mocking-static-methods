using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Uow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

public class UnitOfWorkDbContextProviderTests
{
    private readonly Mock<ILogger<UnitOfWorkDbContextProvider<DbContext>>> _loggerMock;
    private readonly UnitOfWorkDbContextProvider<DbContext> _provider;

    public UnitOfWorkDbContextProviderTests()
    {
        _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<DbContext>>>();
        _provider = new UnitOfWorkDbContextProvider<DbContext>(
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IEfCoreDbContextTypeProvider>()
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
        unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<DbContext>()).Returns(Mock.Of<DbContext>());

        // Act
        _provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Current database does not support transactions. Your database may remain in an inconsistent state in an error case.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }
}
