using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.EntityFrameworkCore;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<ILogger<UnitOfWorkDbContextProvider<DbContext>>> _loggerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IEfCoreDbContextTypeProvider> _efCoreDbContextTypeProviderMock;

        public UnitOfWorkDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<DbContext>>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
        }

        [Fact]
        public async Task CreateDbContextWithTransaction_ShouldLogWarning_WhenBeginTransactionThrowsInvalidOperationException()
        {
            // Arrange
            var provider = new UnitOfWorkDbContextProvider<DbContext>(
                _unitOfWorkManagerMock.Object,
                null,
                null,
                null,
                _efCoreDbContextTypeProviderMock.Object
            );
            provider.Logger = _loggerMock.Object;

            var mockDbContext = new Mock<DbContext>();
            var mockDatabase = new Mock<DatabaseFacade>(mockDbContext.Object);
            mockDatabase.Setup(db => db.BeginTransaction(It.IsAny<System.Data.IsolationLevel>()))
                .Throws<InvalidOperationException>();

            mockDbContext.Setup(c => c.Database).Returns(mockDatabase.Object);

            _unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(_serviceProviderMock.Object);
            _unitOfWorkMock.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = true });
            _unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(null);
            _unitOfWorkMock.Setup(u => u.AddTransactionApi(It.IsAny<string>(), It.IsAny<object>()));

            _serviceProviderMock.Setup(sp => sp.GetRequiredService<DbContext>()).Returns(mockDbContext.Object);

            // Act
            await provider.CreateDbContextWithTransactionAsync(_unitOfWorkMock.Object);

            // Assert
            _loggerMock.Verify(
                log => log.LogWarning(It.Is<string>(s => s.Contains("Current database does not support transactions"))),
                Times.Once);
        }
    }
}
