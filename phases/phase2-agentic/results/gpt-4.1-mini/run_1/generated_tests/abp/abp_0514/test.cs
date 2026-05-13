using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private class DummyDbContext : IEfCoreDbContext { }

        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockConnectionStringResolver = new Mock<IConnectionStringResolver>();
            var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var mockCurrentTenant = new Mock<ICurrentTenant>();
            var mockEfCoreDbContextTypeProvider = new Mock<IEfCoreDbContextTypeProvider>();
            var mockLogger = new Mock<ILogger<UnitOfWorkDbContextProvider<DummyDbContext>>>();

            // Setup UnitOfWorkManager.Current to return a mock unit of work
            mockUnitOfWorkManager.Setup(m => m.Current).Returns(mockUnitOfWork.Object);

            // Setup DisableObsoleteDbContextCreationWarning to false
            var disableWarning = new Mock<IAbpLazy<bool>>();
            disableWarning.SetupGet(x => x.Value).Returns(false);
            var mockUowManagerStatic = new Mock<IUnitOfWorkManager>();
            mockUowManagerStatic.SetupGet(x => x.DisableObsoleteDbContextCreationWarning).Returns(disableWarning.Object);

            // Setup static Uow.UnitOfWorkManager to our mock
            // Since Uow.UnitOfWorkManager is static, we cannot mock it directly.
            // Instead, we will use reflection to set the static property for the test.
            // But since we cannot do that here, we will simulate by setting the static property directly.
            // The user code references Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value
            // We will set Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning to false by reflection.

            // Set UnitOfWork.EnableObsoleteDbContextCreationWarning to true
            typeof(UnitOfWork).GetProperty(nameof(UnitOfWork.EnableObsoleteDbContextCreationWarning))?.SetValue(null, true);

            // Setup EfCoreDbContextTypeProvider to return DummyDbContext type
            mockEfCoreDbContextTypeProvider.Setup(m => m.GetDbContextType(typeof(DummyDbContext))).Returns(typeof(DummyDbContext));

            // Setup ConnectionStringNameAttribute.GetConnStringName to return null or empty string
            // We cannot mock static method, so assume it returns null or empty string
            // So we will mock ResolveConnectionString to return empty string by subclassing

            // Create a subclass to override ResolveConnectionString to return empty string
            var provider = new TestUnitOfWorkDbContextProvider(
                mockUnitOfWorkManager.Object,
                mockConnectionStringResolver.Object,
                mockCancellationTokenProvider.Object,
                mockCurrentTenant.Object,
                mockEfCoreDbContextTypeProvider.Object)
            {
                Logger = mockLogger.Object
            };

            // Setup unitOfWork.GetOrAddDatabaseApi to return a dummy database API with dummy DbContext
            mockUnitOfWork.Setup(uow => uow.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IDatabaseApi>>()))
                .Returns((string key, Func<IDatabaseApi> factory) => factory());

            // Act
            var ex = Record.Exception(() => provider.GetDbContext());

            // Assert
            // We expect no exception because we provide a dummy database API
            Assert.Null(ex);

            // Verify that Logger.LogWarning was called twice
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Length > 0),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));
        }

        private class TestUnitOfWorkDbContextProvider : UnitOfWorkDbContextProvider<DummyDbContext>
        {
            public TestUnitOfWorkDbContextProvider(
                IUnitOfWorkManager unitOfWorkManager,
                IConnectionStringResolver connectionStringResolver,
                ICancellationTokenProvider cancellationTokenProvider,
                ICurrentTenant currentTenant,
                IEfCoreDbContextTypeProvider efCoreDbContextTypeProvider)
                : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, efCoreDbContextTypeProvider)
            {
            }

            protected override string ResolveConnectionString(string connectionStringName)
            {
                return string.Empty;
            }

            protected override DummyDbContext CreateDbContext(IUnitOfWork unitOfWork)
            {
                return new DummyDbContext();
            }
        }
    }
}
