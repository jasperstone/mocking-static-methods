using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.Clients;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow.MongoDB;
using Xunit;

namespace Volo.Abp.Uow.MongoDB;

public class UnitOfWorkMongoDbContextProviderTests
{
    private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
    private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
    private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
    private readonly Mock<ICurrentTenant> _currentTenantMock;
    private readonly Mock<IMongoDbContextTypeProvider> _dbContextTypeProviderMock;
    private readonly Mock<IAbpMongoClientFactory> _mongoClientFactoryMock;
    private readonly Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>> _loggerMock;

    public UnitOfWorkMongoDbContextProviderTests()
    {
        _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        _currentTenantMock = new Mock<ICurrentTenant>();
        _dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
        _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
        _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();
    }

    [Fact]
    public void GetDbContext_Should_LogWarning_When_ObsoleteWarningEnabled()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning = Options.Create(new DisableObsoleteDbContextCreationWarningOptions { Value = false });

        var provider = CreateSut(_loggerMock.Object);

        SetupUnitOfWork();
        SetupDbContextTypeProvider();
        SetupConnectionStringResolver();

        // Act
        provider.GetDbContext();

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
        _loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("at ") && s.Length <= 2048),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    [Fact]
    public void GetDbContext_Should_Not_LogWarning_When_ObsoleteWarningDisabled()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning = Options.Create(new DisableObsoleteDbContextCreationWarningOptions { Value = true });

        var provider = CreateSut(_loggerMock.Object);

        SetupUnitOfWork();
        SetupDbContextTypeProvider();
        SetupConnectionStringResolver();

        // Act
        provider.GetDbContext();

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<string>(),
                It.IsAny<object[]>()
            ),
            Times.Never
        );
    }

    [Fact]
    public void GetDbContext_Should_Not_LogWarning_When_FeatureGloballyDisabled()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = false;

        var provider = CreateSut(_loggerMock.Object);

        SetupUnitOfWork();
        SetupDbContextTypeProvider();
        SetupConnectionStringResolver();

        // Act
        provider.GetDbContext();

        // Assert
        _loggerMock.Verify(
            x => x.LogWarning(
                It.IsAny<string>(),
                It.IsAny<object[]>()
            ),
            Times.Never
        );
    }

    private UnitOfWorkMongoDbContextProvider<TestMongoDbContext> CreateSut(ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>> logger = null)
    {
        return new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
            _unitOfWorkManagerMock.Object,
            _connectionStringResolverMock.Object,
            _cancellationTokenProviderMock.Object,
            _currentTenantMock.Object,
            _dbContextTypeProviderMock.Object,
            _mongoClientFactoryMock.Object
        )
        {
            Logger = logger ?? NullLogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>.Instance
        };
    }

    private void SetupUnitOfWork()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x => x.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IDatabaseApi>>()))
            .Returns(new Mock<MongoDbDatabaseApi>().Object);
        _unitOfWorkManagerMock.Setup(x => x.Current).Returns(unitOfWorkMock.Object);
    }

    private void SetupDbContextTypeProvider()
    {
        _dbContextTypeProviderMock.Setup(x => x.GetDbContextType(It.IsAny<Type>()))
            .Returns(typeof(TestMongoDbContext));
    }

    private void SetupConnectionStringResolver()
    {
        _connectionStringResolverMock.Setup(x => x.ResolveAsync(It.IsAny<Type>()))
            .ReturnsAsync("mongodb://localhost/test");
    }

    // Test context implementation
    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IAbpMongoDbContext ToAbpMongoDbContext() => this;
        public void InitializeDatabase(IMongoDatabase database, MongoClient client, MongoDB.Driver.Core.Bindings.IBindableSessionHandle sessionHandle)
        {
        }
    }
}
