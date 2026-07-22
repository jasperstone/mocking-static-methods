using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task RemoveDuplicateItemsAsync_LogsAndDeletesDuplicates()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();
            var contextMock = new Mock<JellyfinDbContext>();
            var factoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            var routine = new FixIncorrectOwnerIdRelationships(
                new DummyLogger<FixIncorrectOwnerIdRelationships>(), // replace with a dummy logger if needed
                factoryMock.Object,
                libraryManagerMock.Object,
                persistenceServiceMock.Object);

            // Setup context to return a list of duplicate paths
            var baseItems = new FakeDbSet<BaseItem>();
            baseItems.Add(new BaseItem { Id = Guid.NewGuid(), Path = "/path1" });
            baseItems.Add(new BaseItem { Id = Guid.NewGuid(), Path = "/path1" });
            contextMock.Setup(c => c.BaseItems).Returns(baseItems);

            factoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(contextMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            // Verify that LogInformation was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No duplicate items found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }

    // Dummy logger implementation for testing
    public class DummyLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // No-op
        }
    }
}
