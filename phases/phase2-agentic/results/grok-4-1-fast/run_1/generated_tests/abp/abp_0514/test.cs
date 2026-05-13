using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Uow;

[DependsOn(typeof(AbpUnitOfWorkModule))]
public class UnitOfWorkDbContextProviderTestModule : AbpModule
{
}

public class UnitOfWorkDbContextProviderTests : AbpIntegratedTest<UnitOfWorkDbContextProviderTestModule>
{
    [Fact]
    public async Task Should_LogWarning_When_GetDbContext_Called_With_Warning_Enabled()
    {
        // Arrange
        using (var uow = await UnitOfWorkManager.BeginAsync())
        {
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            ServiceProvider.GetRequiredService<IServiceCollection>()
                .AddSingleton(loggerFactoryMock.Object);

            var optionsMock = new Mock<IOptions<IUnitOfWorkDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new UnitOfWorkDefaultOptions { IsTransactional = false });

            // Enable the obsolete warning
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
            Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

            var provider = ServiceProvider.GetRequiredService<IDbContextProvider<TestDbContext>>();

            // Act
            var ex = await Assert.ThrowsAsync<AbpException>(() => provider.GetDbContextAsync());
            
            // We need to call GetDbContext to trigger the warning
            var syncProvider = ServiceProvider.GetRequiredService<IDbContextProvider<TestDbContext>>();
            ((UnitOfWorkDbContextProvider<TestDbContext>)syncProvider).Logger = loggerMock.Object;
            
            syncProvider.GetDbContext();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once
            );

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).StartsWith("   at ")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once
            );
        }
    }

    [Fact]
    public void Should_Not_LogWarning_When_GetDbContext_Called_With_Warning_Disabled()
    {
        // Arrange
        using (var uow = UnitOfWorkManager.Begin())
        {
            var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
            
            var provider = ServiceProvider.GetRequiredService<IDbContextProvider<TestDbContext>>();
            ((UnitOfWorkDbContextProvider<TestDbContext>)provider).Logger = loggerMock.Object;

            // Disable the obsolete warning
            UnitOfWork.EnableObsoleteDbContextCreationWarning = false;

            // Act
            provider.GetDbContext();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never
            );
        }
    }
}

public interface TestDbContext : IEfCoreDbContext
{
}
