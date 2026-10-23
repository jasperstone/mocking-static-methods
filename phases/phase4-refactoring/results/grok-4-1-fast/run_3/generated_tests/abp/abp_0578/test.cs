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
    public void GetDbContext_Should_LogWarning_When_ObsoleteWarningEnabled()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(u => u.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
        _mockUnitOfWorkManager.Setup(m => m.Current).Returns(mockUow.Object);

        _mockDbContextTypeProvider.Setup(p => p.GetDbContextType(typeof(TestMongoDbContext)))
            .Returns(typeof(TestMongoDbContext));

        _mockConnectionStringResolver.Setup(r => r.ResolveAsync(It.IsAny<Type>()))
            .ReturnsAsync("mongodb://localhost/test");

        var provider = CreateSut();

        // Act
        provider.GetDbContext();

        // Assert
        _mockLogger.Verify(
            l => l.LogWarning(
                It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))),
            Times.Once);
        _mockLogger.Verify(
            l => l.LogWarning(It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Fact]
    public void GetDbContext_Should_Not_LogWarning_When_ObsoleteWarningDisabled()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = true;

        var mockUow = new Mock<IUnitOfWork>();
        _mockUnitOfWorkManager.Setup(m => m.Current).Returns(mockUow.Object);

        var provider = CreateSut();

        // Act
        provider.GetDbContext();

        // Assert
        _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void GetDbContext_Should_ThrowAbpException_When_NoCurrentUnitOfWork()
    {
        // Arrange
        _mockUnitOfWorkManager.Setup(m => m.Current).Returns((IUnitOfWork)null);

        var provider = CreateSut();

        // Act & Assert
        var exception = Assert.Throws<AbpException>(() => provider.GetDbContext());
        Assert.Contains("unit of work", exception.Message);
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

    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IMongoClient Client { get; set; } = null!;
        public IMongoDatabase Database { get; set; } = null!;
        public IClientSessionHandle? SessionHandle { get; set; }

        public IAbpMongoDbContext ToAbpMongoDbContext() => this;

        public IMongoCollection<T> Collection<T>() where T : class => throw new NotImplementedException();

        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle? sessionHandle)
        {
            Database = database;
            Client = client;
            SessionHandle = sessionHandle;
        }

        public void Dispose() { }
    }
}
