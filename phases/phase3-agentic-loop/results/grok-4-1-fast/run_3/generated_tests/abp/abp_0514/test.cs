using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests;

public class UnitOfWorkDbContextProviderTests
{
    [Fact]
    public void GetDbContext_Should_LogWarning_When_WarningEnabled()
    {
        // Arrange
        var logger = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
        var uow = new Mock<IUnitOfWork>();
        unitOfWorkManager.Setup(m => m.Current).Returns(uow.Object);

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            unitOfWorkManager.Object,
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IEfCoreDbContextTypeProvider>()
        )
        {
            Logger = logger.Object
        };

        // Mock the static condition using reflection
        SetStaticBool(typeof(UnitOfWork), "EnableObsoleteDbContextCreationWarning", true);
        SetStaticBool(typeof(UowUnitOfWorkManager), "DisableObsoleteDbContextCreationWarning", false);

        // Act
        provider.GetDbContext();

        // Assert - verify both LogWarning calls (line 57 is the second one)
        logger.Verify(l => l.LogWarning(
            It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))), Times.Once);
        logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void GetDbContext_Should_Not_LogWarning_When_WarningDisabled()
    {
        // Arrange
        var logger = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
        var uow = new Mock<IUnitOfWork>();
        unitOfWorkManager.Setup(m => m.Current).Returns(uow.Object);

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            unitOfWorkManager.Object,
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IEfCoreDbContextTypeProvider>()
        )
        {
            Logger = logger.Object
        };

        // Mock the static condition using reflection
        SetStaticBool(typeof(UnitOfWork), "EnableObsoleteDbContextCreationWarning", false);

        // Act
        provider.GetDbContext();

        // Assert
        logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Never);
    }

    private static void SetStaticBool(Type type, string fieldName, bool value)
    {
        var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        field?.SetValue(null, value);
    }
}

public interface TestDbContext : IEfCoreDbContext
{
}
