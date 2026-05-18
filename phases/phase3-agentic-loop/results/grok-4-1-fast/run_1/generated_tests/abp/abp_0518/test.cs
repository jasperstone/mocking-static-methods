using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests;

public class UnitOfWorkDbContextProviderTests
{
    [Fact]
    public void CreateDbContextWithTransaction_LogsWarning_WhenTransactionNotSupported()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dbContextMock = new Mock<TestDbContext>();

        dbContextMock.Setup(x => x.Database.BeginTransaction(It.IsAny<IsolationLevel>()))
                     .Throws(new InvalidOperationException("Transactions not supported"));

        serviceProviderMock.Setup(sp => sp.GetRequiredService<TestDbContext>())
                          .Returns(dbContextMock.Object);

        unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
        unitOfWorkMock.Setup(u => u.Options).Returns(new AbpUnitOfWorkOptions { IsTransactional = true });

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
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Current database does not support transactions")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        Assert.NotNull(result);
    }
}

public class TestDbContext : DbContext, IEfCoreDbContext
{
    private readonly DbContextOptions<TestDbContext> _options;

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("Test");
    }

    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
        => throw new NotImplementedException();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
