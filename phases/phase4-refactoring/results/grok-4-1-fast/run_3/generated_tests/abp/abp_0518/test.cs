using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Uow.Tests;

public class UnitOfWorkDbContextProviderTests
{
    private const string TransactionsNotSupportedWarningMessage = "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.";

    [Fact]
    public void Should_LogWarning_When_BeginTransaction_Throws_InvalidOperationException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dbContextOptionsMock = new Mock<DbContextOptions<TestDbContext>>();
        var dbContextMock = new Mock<TestDbContext>(dbContextOptionsMock.Object);

        dbContextMock
            .Setup(x => x.Database.BeginTransaction())
            .Throws(new InvalidOperationException("Transactions not supported"));

        serviceProviderMock
            .Setup(x => x.GetRequiredService<TestDbContext>())
            .Returns(dbContextMock.Object);

        unitOfWorkMock
            .Setup(x => x.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        unitOfWorkMock
            .SetupGet(x => x.Options)
            .Returns(new UnitOfWorkOptions { IsTransactional = true });

        unitOfWorkMock
            .Setup(x => x.FindTransactionApi(It.IsAny<string>()))
            .Returns((EfCoreTransactionApi)null);

        var provider = new TestableUnitOfWorkDbContextProvider<TestDbContext>(
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IEfCoreDbContextTypeProvider>(),
            loggerMock.Object
        );

        // Act
        var result = provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => v.ToString()!.Contains(TransactionsNotSupportedWarningMessage))),
            Times.Once
        );

        Assert.Same(dbContextMock.Object, result);
    }

    [Fact]
    public void Should_LogWarning_When_BeginTransaction_Throws_NotSupportedException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dbContextOptionsMock = new Mock<DbContextOptions<TestDbContext>>();
        var dbContextMock = new Mock<TestDbContext>(dbContextOptionsMock.Object);

        dbContextMock
            .Setup(x => x.Database.BeginTransaction())
            .Throws(new NotSupportedException("Transactions not supported"));

        serviceProviderMock
            .Setup(x => x.GetRequiredService<TestDbContext>())
            .Returns(dbContextMock.Object);

        unitOfWorkMock
            .Setup(x => x.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        unitOfWorkMock
            .SetupGet(x => x.Options)
            .Returns(new UnitOfWorkOptions { IsTransactional = true });

        unitOfWorkMock
            .Setup(x => x.FindTransactionApi(It.IsAny<string>()))
            .Returns((EfCoreTransactionApi)null);

        var provider = new TestableUnitOfWorkDbContextProvider<TestDbContext>(
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IEfCoreDbContextTypeProvider>(),
            loggerMock.Object
        );

        // Act
        var result = provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => v.ToString()!.Contains(TransactionsNotSupportedWarningMessage))),
            Times.Once
        );

        Assert.Same(dbContextMock.Object, result);
    }

    public class TestDbContext : DbContext, IEfCoreDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public Task<int> SaveChangesOnDbContextAsync(bool saveNothrow, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class TestableUnitOfWorkDbContextProvider<TDbContext> : UnitOfWorkDbContextProvider<TDbContext>
        where TDbContext : IEfCoreDbContext
    {
        public TestableUnitOfWorkDbContextProvider(
            IUnitOfWorkManager unitOfWorkManager,
            IConnectionStringResolver connectionStringResolver,
            ICancellationTokenProvider cancellationTokenProvider,
            ICurrentTenant currentTenant,
            IEfCoreDbContextTypeProvider efCoreDbContextTypeProvider,
            ILogger<UnitOfWorkDbContextProvider<TDbContext>> logger)
            : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, efCoreDbContextTypeProvider)
        {
            Logger = logger;
        }

        public new virtual TDbContext CreateDbContextWithTransaction(IUnitOfWork unitOfWork)
        {
            return base.CreateDbContextWithTransaction(unitOfWork);
        }
    }
}
