using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests;

public class UnitOfWorkDbContextProviderTests
{
    private const string TransactionsNotSupportedWarningMessage = 
        "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.";

    [Fact]
    public void CreateDbContextWithTransaction_ShouldLogWarning_WhenTransactionNotSupported()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(CreateServiceProvider());
        unitOfWorkMock.Setup(u => u.Options).Returns(new Mock<IUnitOfWorkOptions>().Object);
        unitOfWorkMock.Setup(u => u.Options.IsTransactional).Returns(true);
        unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((EfCoreTransactionApi)null);

        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            unitOfWorkManagerMock.Object,
            connectionStringResolverMock.Object,
            cancellationTokenProviderMock.Object,
            Mock.Of<ICurrentTenant>(),
            efCoreDbContextTypeProviderMock.Object
        );
        provider.Logger = loggerMock.Object;

        // Mock DbContext to throw InvalidOperationException on Database.BeginTransaction
        var dbContextMock = new Mock<TestDbContext>();
        var databaseMock = new Mock<IRelationalDatabase>();
        dbContextMock.Setup(x => x.Database).Returns(databaseMock.Object);
        databaseMock.Setup(x => x.BeginTransaction(It.IsAny<IsolationLevel>()))
                    .Throws(new InvalidOperationException("Transactions not supported"));
        databaseMock.Setup(x => x.BeginTransaction())
                    .Throws(new InvalidOperationException("Transactions not supported"));

        unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<TestDbContext>())
                      .Returns(dbContextMock.Object);

        // Act & Assert
        var result = provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

        loggerMock.Verify(
            x => x.LogWarning(TransactionsNotSupportedWarningMessage),
            Times.Once
        );
        Assert.NotNull(result);
    }

    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services.BuildServiceProvider();
    }
}

public interface TestDbContext : IEfCoreDbContext
{
}
