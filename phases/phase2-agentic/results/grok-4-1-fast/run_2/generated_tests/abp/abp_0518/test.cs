using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Uow;

[DependsOn(typeof(Volo.Abp.EntityFrameworkCore.AbpEntityFrameworkCoreModule))]
public class UnitOfWorkDbContextProviderTests : AbpIntegratedTest<AbpEntityFrameworkCoreTestModule>
{
    [Fact]
    public void LogWarning_ShouldBeCalled_WhenTransactionBeginThrowsInvalidOperationException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dbContextMock = new Mock<TestDbContext>(serviceProviderMock.Object);
        var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
        dbContextMock.Setup(c => c.Database).Returns(databaseMock.Object);

        databaseMock
            .Setup(db => db.BeginTransaction())
            .Throws(new InvalidOperationException("Transactions not supported"));

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<TestDbContext>())
            .Returns(dbContextMock.Object);

        unitOfWorkMock
            .Setup(uow => uow.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        unitOfWorkMock
            .Setup(uow => uow.Options)
            .Returns(new UnitOfWorkOptions { IsTransactional = true });

        unitOfWorkMock
            .Setup(uow => uow.FindTransactionApi(It.IsAny<string>()))
            .Returns((EfCoreTransactionApi)null);

        unitOfWorkMock
            .Setup(uow => uow.AddTransactionApi(It.IsAny<string>(), It.IsAny<EfCoreTransactionApi>()))
            .Returns(default);

        // Create provider with mocked logger
        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IEfCoreDbContextTypeProvider>()
        );
        provider.Logger = loggerMock.Object;

        // Use reflection to set private fields for testing
        typeof(UnitOfWorkDbContextProvider<TestDbContext>)
            .GetField("UnitOfWorkManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(provider, unitOfWorkMock.Object);

        // Act
        var action = () => typeof(UnitOfWorkDbContextProvider<TestDbContext>)
            .GetMethod("CreateDbContextWithTransaction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(provider, new object[] { unitOfWorkMock.Object });

        // Assert
        loggerMock.Verify(
            logger => logger.LogWarning(
                It.Is<string>(msg => msg == "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.")),
            Times.Once);
    }

    [Fact]
    public void LogWarning_ShouldBeCalled_WhenTransactionBeginThrowsNotSupportedException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dbContextMock = new Mock<TestDbContext>(serviceProviderMock.Object);
        var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
        dbContextMock.Setup(c => c.Database).Returns(databaseMock.Object);

        databaseMock
            .Setup(db => db.BeginTransaction())
            .Throws(new NotSupportedException("Transactions not supported"));

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<TestDbContext>())
            .Returns(dbContextMock.Object);

        unitOfWorkMock
            .Setup(uow => uow.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        unitOfWorkMock
            .Setup(uow => uow.Options)
            .Returns(new UnitOfWorkOptions { IsTransactional = true });

        unitOfWorkMock
            .Setup(uow => uow.FindTransactionApi(It.IsAny<string>()))
            .Returns((EfCoreTransactionApi)null);

        // Create provider with mocked logger
        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IEfCoreDbContextTypeProvider>()
        );
        provider.Logger = loggerMock.Object;

        // Use reflection to set private fields
        typeof(UnitOfWorkDbContextProvider<TestDbContext>)
            .GetField("UnitOfWorkManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(provider, unitOfWorkMock.Object);

        // Act
        var action = () => typeof(UnitOfWorkDbContextProvider<TestDbContext>)
            .GetMethod("CreateDbContextWithTransaction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(provider, new object[] { unitOfWorkMock.Object });

        // Assert
        loggerMock.Verify(
            logger => logger.LogWarning(
                It.Is<string>(msg => msg.Contains("Current database does not support transactions")),
                Times.Once);
    }
}

public class TestDbContext : DbContext, IEfCoreDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public string DefaultConnectionStringOrName => "Test";
}
