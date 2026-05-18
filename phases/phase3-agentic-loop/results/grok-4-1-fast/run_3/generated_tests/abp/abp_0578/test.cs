using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using Volo.Abp.Threading;
using Volo.Abp.Uow.MongoDB;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests;

public class UnitOfWorkMongoDbContextProviderTest
{
    [Fact]
    public void GetDbContext_ShouldLogWarning_WhenObsoleteWarningEnabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        unitOfWorkManagerMock.Setup(u => u.Current).Returns(new Mock<IUnitOfWork>().Object);
        var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
        dbContextTypeProviderMock.Setup(x => x.GetDbContextType(typeof(TestMongoDbContext)))
            .Returns(typeof(TestMongoDbContext));

        var provider = new TestableUnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
            unitOfWorkManagerMock.Object,
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            dbContextTypeProviderMock.Object,
            Mock.Of<IAbpMongoClientFactory>()
        )
        {
            Logger = loggerMock.Object
        };

        // Enable the obsolete warning
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

        // Act
        provider.GetDbContext();

        // Assert - verify first LogWarning call (deprecation message)
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );

        // Assert - verify second LogWarning call (stack trace)
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("at ") || ((string)v).Contains("Stack trace")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void GetDbContext_ShouldNotLogWarning_WhenObsoleteWarningDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        unitOfWorkManagerMock.Setup(u => u.Current).Returns(new Mock<IUnitOfWork>().Object);
        var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
        dbContextTypeProviderMock.Setup(x => x.GetDbContextType(typeof(TestMongoDbContext)))
            .Returns(typeof(TestMongoDbContext));

        var provider = new TestableUnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
            unitOfWorkManagerMock.Object,
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            dbContextTypeProviderMock.Object,
            Mock.Of<IAbpMongoClientFactory>()
        )
        {
            Logger = loggerMock.Object
        };

        // Disable the obsolete warning
        UnitOfWork.EnableObsoleteDbContextCreationWarning = false;

        // Act
        provider.GetDbContext();

        // Assert
        loggerMock.VerifyNoOtherCalls();
    }
}

public class TestMongoDbContext : IAbpMongoDbContext
{
    public object Database => null!;
    public object Client => null!;
    public object Collection<T>() => null!;
    public object? SessionHandle => null;
    public IAbpMongoDbContext ToAbpMongoDbContext() => this;
}

public class TestableUnitOfWorkMongoDbContextProvider<TMongoDbContext> : UnitOfWorkMongoDbContextProvider<TMongoDbContext>
    where TMongoDbContext : IAbpMongoDbContext
{
    public TestableUnitOfWorkMongoDbContextProvider(
        IUnitOfWorkManager unitOfWorkManager,
        IConnectionStringResolver connectionStringResolver,
        ICancellationTokenProvider cancellationTokenProvider,
        ICurrentTenant currentTenant,
        IMongoDbContextTypeProvider dbContextTypeProvider,
        IAbpMongoClientFactory mongoClientFactory)
        : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, dbContextTypeProvider, mongoClientFactory)
    {
    }

    protected override string ResolveConnectionString(Type targetDbContextType)
    {
        return "mongodb://localhost/test";
    }

    [Obsolete("Use CreateDbContextAsync")]
    protected override TMongoDbContext CreateDbContext(IUnitOfWork unitOfWork, object mongoUrl, string databaseName)
    {
        return Activator.CreateInstance<TMongoDbContext>()!;
    }
}
