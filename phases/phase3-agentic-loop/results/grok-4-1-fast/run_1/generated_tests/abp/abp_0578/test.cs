using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
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
        UnitOfWorkManager.DisableObsoleteDbContextCreationWarning = 
            new Mock<IAbpLazyServiceProvider<string, bool>>().Object;

        var provider = CreateSut();

        SetupMocksForGetDbContext();

        // Act
        provider.GetDbContext();

        // Assert
        _mockLogger.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))),
            Times.Once);
        
        _mockLogger.Verify(
            x => x.LogWarning(It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Fact]
    public void GetDbContext_Should_Not_LogWarning_When_ObsoleteWarningDisabled()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = false;

        var provider = CreateSut();

        SetupMocksForGetDbContext();

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

    private void SetupMocksForGetDbContext()
    {
        var unitOfWork = new Mock<IUnitOfWork>().Object;
        _mockUnitOfWorkManager.Setup(x => x.Current).Returns(unitOfWork);

        _mockDbContextTypeProvider.Setup(x => x.GetDbContextType(typeof(TestMongoDbContext)))
            .Returns(typeof(TestMongoDbContext));

        _mockConnectionStringResolver.Setup(x => x.ResolveAsync(It.IsAny<Type>()))
            .ReturnsAsync("mongodb://localhost/test");
    }

    // Minimal implementation for compilation
    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IMongoDatabase Database { get; set; }
        public IMongoClient Client { get; set; }
        public IBindableHandle SessionHandle { get; set; }

        public IMongoCollection<TDocument> Collection<TDocument>(string name = null) where TDocument : class
        {
            throw new NotImplementedException();
        }

        public IAbpMongoDbContext ToAbpMongoDbContext() => this;

        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IBindableHandle sessionHandle)
        {
            Database = database;
            Client = client;
            SessionHandle = sessionHandle;
        }
    }
}
