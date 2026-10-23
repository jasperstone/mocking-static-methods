using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProvider_LoggerExtensions_Tests
    {
        [Fact]
        public void CreateDbContextWithTransaction_LogsWarning_WhenTransactionNotSupported()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<FakeDbContext>>>();

            var provider = new TestUnitOfWorkDbContextProvider(loggerMock.Object);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((object)null);
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(CreateServiceProviderReturningFakeDbContext());
            unitOfWorkMock.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsolationLevel = null, IsTransactional = true });
            unitOfWorkMock.Setup(u => u.AddTransactionApi(It.IsAny<string>(), It.IsAny<object>()));

            // Act
            var dbContext = provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Current database does not support transactions")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.NotNull(dbContext);
        }

        private static IServiceProvider CreateServiceProviderReturningFakeDbContext()
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(FakeDbContext))).Returns(new FakeDbContext());
            return serviceProviderMock.Object;
        }

        private class TestUnitOfWorkDbContextProvider : UnitOfWorkDbContextProvider<FakeDbContext>
        {
            public TestUnitOfWorkDbContextProvider(ILogger<UnitOfWorkDbContextProvider<FakeDbContext>> logger)
                : base(null, null, null, null, null)
            {
                Logger = logger;
            }

            public new FakeDbContext CreateDbContextWithTransaction(IUnitOfWork unitOfWork)
            {
                return base.CreateDbContextWithTransaction(unitOfWork);
            }
        }

        private class FakeDbContext : IEfCoreDbContext
        {
            public FakeDatabase Database { get; } = new FakeDatabase();
        }

        private class FakeDatabase
        {
            public IDbContextTransaction BeginTransaction()
            {
                throw new NotSupportedException();
            }

            public IDbContextTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
            {
                throw new NotSupportedException();
            }
        }

        private interface IUnitOfWork
        {
            IServiceProvider ServiceProvider { get; }
            IUnitOfWorkOptions Options { get; }
            object FindTransactionApi(string key);
            void AddTransactionApi(string key, object transactionApi);
        }

        private interface IUnitOfWorkOptions
        {
            bool IsTransactional { get; }
            System.Data.IsolationLevel? IsolationLevel { get; }
        }

        private class UnitOfWorkOptions : IUnitOfWorkOptions
        {
            public bool IsTransactional { get; set; }
            public System.Data.IsolationLevel? IsolationLevel { get; set; }
        }

        private interface IEfCoreDbContext { }
        private interface IDbContextTransaction { }
    }
}
