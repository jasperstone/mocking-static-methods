using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.Clients;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.MongoDB;
using Xunit;

namespace Volo.Abp.Uow.MongoDB;

public class UnitOfWorkMongoDbContextProviderTests
{
    private readonly Mock<IUnitOfWorkManager> _mockUnitOfWorkManager;
    private readonly Mock<IConnectionStringResolver> _mockConnectionStringResolver;
    private readonly Mock<ICancellationTokenProvider> _mockCancellationTokenProvider;
    private readonly Mock<ICurrentTenant> _mockCurrentTenant;
    private readonly Mock<IMongoDbContextTypeProvider> _mockDbContextTypeProvider;
    private readonly Mock<IAbpMongoClientFactory> _mockMongoClientFactory;
    private readonly Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>> _mockLogger;

    public UnitOfWorkMongoDbContextProviderTests()
    {
        _mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
        _mockConnectionStringResolver = new Mock<IConnectionStringResolver>();
        _mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
        _mockCurrentTenant = new Mock<ICurrentTenant>();
        _mockDbContextTypeProvider = new Mock<IMongoDbContextTypeProvider>();
        _mockMongoClientFactory = new Mock<IAbpMongoClientFactory>();
        _mockLogger = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();
    }

    [Fact]
    public void GetDbContext_ShouldLogWarning_WhenObsoleteWarningEnabled()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

        var provider = CreateSut();

        SetupUnitOfWork();
        SetupDbContextTypeProvider();
        SetupConnectionStringResolver();

        // Act
        provider.GetDbContext();

        // Assert
        _mockLogger.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))),
            Times.Once);
        _mockLogger.Verify(
            x => x.LogWarning(It.IsAny<string>()),
            Times.Exactly(2)); // First for deprecation message, second for stack trace
    }

    [Fact]
    public void GetDbContext_ShouldNotLogWarning_WhenObsoleteWarningDisabled()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = true;

        var provider = CreateSut();

        SetupUnitOfWork();

        // Act
        provider.GetDbContext();

        // Assert
        _mockLogger.Verify(
            x => x.LogWarning(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void GetDbContext_ShouldNotLogWarning_WhenEnableObsoleteWarningDisabled()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = false;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

        var provider = CreateSut();

        SetupUnitOfWork();

        // Act
        provider.GetDbContext();

        // Assert
        _mockLogger.Verify(
            x => x.LogWarning(It.IsAny<string>()),
            Times.Never);
    }

    private UnitOfWorkMongoDbContextProvider<TestMongoDbContext> CreateSut()
    {
        return new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
            _mockUnitOfWorkManager.Object,
            _mockConnectionStringResolver.Object,
            _mockCancellationTokenProvider.Object,
            _mockCurrentTenant.Object,
            _mockDbContextTypeProvider.Object,
            _mockMongoClientFactory.Object)
        {
            Logger = _mockLogger.Object
        };
    }

    private void SetupUnitOfWork()
    {
        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(x => x.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
        _mockUnitOfWorkManager.Setup(x => x.Current).Returns(mockUow.Object);
    }

    private void SetupDbContextTypeProvider()
    {
        _mockDbContextTypeProvider.Setup(x => x.GetDbContextType(typeof(TestMongoDbContext)))
            .Returns(typeof(TestMongoDbContext));
    }

    private void SetupConnectionStringResolver()
    {
        _mockConnectionStringResolver.Setup(x => x.ResolveAsync(It.IsAny<Type>()))
            .ReturnsAsync("mongodb://localhost/test");
    }

    // Minimal TestMongoDbContext - we don't actually use it in these tests
    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IMongoDatabase Database { get; set; } = null!;
        public IMongoClient Client { get; set; } = null!;
        public IClientSessionHandle? SessionHandle { get; set; }

        public IMongoCollection<T> Collection<T>() where T : class => throw new NotImplementedException();
        
        public IAbpMongoDbContext ToAbpMongoDbContext() => this;
        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle? sessionHandle)
        {
            Database = database;
            Client = client;
            SessionHandle = sessionHandle;
        }
    }
}
