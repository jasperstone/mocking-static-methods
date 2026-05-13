using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TestApp.TestApp.Domain;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Uow;

[Fact]
public class UnitOfWorkDbContextProvider_LogWarning_Tests
{
    [Fact]
    public async Task Should_LogWarning_When_GetDbContext_Called_With_Warning_Enabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var currentTenantMock = new Mock<ICurrentTenant>();
        var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

        connectionStringResolverMock
            .Setup(x => x.ResolveAsync(It.IsAny<string>()))
            .ReturnsAsync("TestConnectionString");

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x => x.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
        unitOfWorkManagerMock.Setup(x => x.Current).Returns(unitOfWorkMock.Object);

        // Enable the warning
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            unitOfWorkManagerMock.Object,
            connectionStringResolverMock.Object,
            cancellationTokenProviderMock.Object,
            currentTenantMock.Object,
            efCoreDbContextTypeProviderMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        await provider.GetDbContextAsync(); // Ensure UoW is set up
        var result = provider.GetDbContext();

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(msg => msg.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                It.IsAny<Exception>(),
                It.IsAny<object[]>()
            ),
            Times.Once
        );

        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(msg => msg.Contains(Environment.StackTrace.Truncate(2048))),
                It.IsAny<Exception>(),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void Should_Not_LogWarning_When_GetDbContext_Called_With_Warning_Disabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var currentTenantMock = new Mock<ICurrentTenant>();
        var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

        connectionStringResolverMock
            .Setup(x => x.ResolveAsync(It.IsAny<string>()))
            .ReturnsAsync("TestConnectionString");

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x => x.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
        unitOfWorkManagerMock.Setup(x => x.Current).Returns(unitOfWorkMock.Object);

        // Disable the warning
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = true;

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            unitOfWorkManagerMock.Object,
            connectionStringResolverMock.Object,
            cancellationTokenProviderMock.Object,
            currentTenantMock.Object,
            efCoreDbContextTypeProviderMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        provider.GetDbContext();

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<string>(),
                It.IsAny<Exception>(),
                It.IsAny<object[]>()
            ),
            Times.Never
        );
    }

    [Fact]
    public void Should_Not_LogWarning_When_WarningGloballyDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var currentTenantMock = new Mock<ICurrentTenant>();
        var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

        connectionStringResolverMock
            .Setup(x => x.ResolveAsync(It.IsAny<string>()))
            .ReturnsAsync("TestConnectionString");

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x => x.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
        unitOfWorkManagerMock.Setup(x => x.Current).Returns(unitOfWorkMock.Object);

        // Globally disable the warning
        UnitOfWork.EnableObsoleteDbContextCreationWarning = false;

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            unitOfWorkManagerMock.Object,
            connectionStringResolverMock.Object,
            cancellationTokenProviderMock.Object,
            currentTenantMock.Object,
            efCoreDbContextTypeProviderMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        provider.GetDbContext();

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<string>(),
                It.IsAny<Exception>(),
                It.IsAny<object[]>()
            ),
            Times.Never
        );
    }
}

// Dummy DbContext for generic constraint
public class TestDbContext : IEfCoreDbContext
{
    public string DefaultDbContextName { get; set; } = "Default";
}
