using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests;

public class UnitOfWorkDbContextProviderTests
{
    [Fact]
    public void CreateDbContextWithTransaction_Should_LogWarning_When_BeginTransaction_Throws_InvalidOperationException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        loggerMock.SetupAllProperties();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var unitOfWorkOptionsMock = new Mock<IUnitOfWorkOptions>();
        unitOfWorkOptionsMock.Setup(o => o.IsTransactional).Returns(true);
        unitOfWorkMock.Setup(u => u.Options).Returns(unitOfWorkOptionsMock.Object);
        unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((EfCoreTransactionApi)null);
        unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(CreateServiceProviderWithDbContext(true));

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<Volo.Abp.Threading.ICancellationTokenProvider>(),
            Mock.Of<Volo.Abp.MultiTenancy.ICurrentTenant>(),
            Mock.Of<IEfCoreDbContextTypeProvider>()
        );
        provider.Logger = loggerMock.Object;

        // Act - using reflection to call protected method
        var method = typeof(UnitOfWorkDbContextProvider<TestDbContext>)
            .GetMethod("CreateDbContextWithTransaction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        method.Invoke(provider, new object[] { unitOfWorkMock.Object });

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.",
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void CreateDbContextWithTransaction_Should_LogWarning_When_BeginTransaction_Throws_NotSupportedException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        loggerMock.SetupAllProperties();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var unitOfWorkOptionsMock = new Mock<IUnitOfWorkOptions>();
        unitOfWorkOptionsMock.Setup(o => o.IsTransactional).Returns(true);
        unitOfWorkMock.Setup(u => u.Options).Returns(unitOfWorkOptionsMock.Object);
        unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((EfCoreTransactionApi)null);
        unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(CreateServiceProviderWithDbContext(false, true));

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<Volo.Abp.Threading.ICancellationTokenProvider>(),
            Mock.Of<Volo.Abp.MultiTenancy.ICurrentTenant>(),
            Mock.Of<IEfCoreDbContextTypeProvider>()
        );
        provider.Logger = loggerMock.Object;

        // Act
        var method = typeof(UnitOfWorkDbContextProvider<TestDbContext>)
            .GetMethod("CreateDbContextWithTransaction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        method.Invoke(provider, new object[] { unitOfWorkMock.Object });

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.",
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    private IServiceProvider CreateServiceProviderWithDbContext(bool throwInvalidOperation = false, bool throwNotSupported = false)
    {
        var dbContextMock = new Mock<TestDbContext>();
        var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
        var dbConnectionMock = new Mock<DbConnection>();

        databaseMock.Setup(d => d.GetDbConnection()).Returns(dbConnectionMock.Object);

        if (throwInvalidOperation)
        {
            databaseMock.Setup(d => d.BeginTransaction()).Throws(new InvalidOperationException("Transactions not supported"));
        }
        else if (throwNotSupported)
        {
            databaseMock.Setup(d => d.BeginTransaction()).Throws(new NotSupportedException("Transactions not supported"));
        }

        dbContextMock.Setup(c => c.Database).Returns(databaseMock.Object);

        var services = new ServiceCollection();
        services.AddSingleton(dbContextMock.Object);
        return services.BuildServiceProvider();
    }
}

public interface TestDbContext : IEfCoreDbContext
{
}
