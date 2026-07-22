using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

        var provider = CreateSut();

        SetupMocksForGetDbContext();

        // Act
        provider.GetDbContext();

        // Assert
        _mockLogger.Verify(l => l.LogWarning(
            It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))), Times.Once);
        _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void GetDbContext_Should_Not_LogWarning_When_ObsoleteWarningDisabled()
    {
        // Arrange
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = true;

        var provider = CreateSut();

        SetupMocksForGetDbContext();

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
        Assert.Contains("unit of work", exception.Message, StringComparison.OrdinalIgnoreCase);
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
        var mockUow = new Mock<IUnitOfWork>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<TestMongoDbContext>())
            .Returns(new TestMongoDbContext());
        mockUow.Setup(u => u.ServiceProvider).Returns(mockServiceProvider.Object);
        
        var mockDatabaseApi = new Mock<MongoDbDatabaseApi>();
        mockDatabaseApi.Setup(x => x.DbContext).Returns(new TestMongoDbContext());
        
        mockUow.Setup(u => u.GetOrAddDatabaseApi(
            It.IsAny<string>(), 
            It.IsAny<Func<IDatabaseApi>>()))
            .Returns(mockDatabaseApi.Object);
            
        _mockUnitOfWorkManager.Setup(m => m.Current).Returns(mockUow.Object);
        _mockDbContextTypeProvider.Setup(p => p.GetDbContextType(typeof(TestMongoDbContext)))
            .Returns(typeof(TestMongoDbContext));
        _mockConnectionStringResolver.Setup(r => r.ResolveAsync(It.IsAny<Type>()))
            .ReturnsAsync("mongodb://localhost/test");
    }

    public class TestMongoDbContext : IAbpMongoDbContext
    {
        public IMongoClient Client { get; set; }
        public IMongoDatabase Database { get; set; }
        public IClientSessionHandle SessionHandle { get; set; }

        public IMongoCollection<T> Collection<T>() where T : class => throw new NotImplementedException();

        public IMongoDbContextExecutionContext ToAbpMongoDbContext() => throw new NotImplementedException();
    }
}
