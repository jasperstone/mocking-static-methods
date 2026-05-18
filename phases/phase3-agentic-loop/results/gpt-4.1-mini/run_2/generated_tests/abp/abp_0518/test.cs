using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow.EntityFrameworkCore;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private class TestDbContext : DbContext, IEfCoreDbContext
        {
            public TestDbContext(DbContextOptions options) : base(options) { }
        }

        [Fact]
        public void CreateDbContextWithTransaction_LogsWarning_WhenTransactionNotSupported()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var optionsMock = new Mock<IUnitOfWorkOptions>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

            var dbContextOptions = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("TestDb").Options;
            var dbContext = new TestDbContext(dbContextOptions);

            // Setup service provider to return our dbContext
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(TestDbContext))).Returns(dbContext);
            serviceProvider.Setup(sp => sp.GetRequiredService<TestDbContext>()).Returns(dbContext);

            // Setup unitOfWork
            unitOfWorkMock.Setup(uow => uow.ServiceProvider).Returns(serviceProvider.Object);
            unitOfWorkMock.Setup(uow => uow.FindTransactionApi(It.IsAny<string>())).Returns(null);
            unitOfWorkMock.Setup(uow => uow.Options).Returns(optionsMock.Object);
            unitOfWorkMock.Setup(uow => uow.AddTransactionApi(It.IsAny<string>(), It.IsAny<object>()));

            // Setup options to have no isolation level
            optionsMock.Setup(o => o.IsolationLevel).Returns((System.Data.IsolationLevel?)null);

            // Setup logger mock to verify LogWarning call
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();

            // Setup DbContext.Database.BeginTransaction to throw InvalidOperationException
            var databaseMock = new Mock<DatabaseFacade>(dbContext);
            databaseMock.Setup(d => d.BeginTransaction()).Throws<InvalidOperationException>();
            // Replace the Database property with our mock
            var dbContextMock = new Mock<TestDbContext>(dbContextOptions) { CallBase = true };
            dbContextMock.SetupGet(d => d.Database).Returns(databaseMock.Object);

            // Setup service provider to return mocked dbContext with throwing BeginTransaction
            serviceProvider.Setup(sp => sp.GetRequiredService<TestDbContext>()).Returns(dbContextMock.Object);

            var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<IConnectionStringResolver>(),
                cancellationTokenProviderMock.Object,
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IEfCoreDbContextTypeProvider>()
            );
            provider.Logger = loggerMock.Object;

            // Act
            var result = provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

            // Assert
            Assert.NotNull(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Current database does not support transactions")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
