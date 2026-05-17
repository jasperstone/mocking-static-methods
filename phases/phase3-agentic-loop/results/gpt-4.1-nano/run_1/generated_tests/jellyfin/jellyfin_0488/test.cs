using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Tests.Migrations
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        private class DummyLogger<T> : ILogger<T>
        {
            public List<string> LogMessages { get; } = new List<string>();
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LogMessages.Add(formatter(state, exception));
            }
        }

        private class DummyLibraryManager : ILibraryManager
        {
            public List<Guid> DeletedIds { get; } = new List<Guid>();
            public List<object> Items { get; } = new List<object>();
            public void DeleteItemsUnsafeFast(List<object> items)
            {
                foreach (var item in items)
                {
                    var idProp = item.GetType().GetProperty("Id");
                    if (idProp != null)
                    {
                        var id = (Guid)idProp.GetValue(item);
                        DeletedIds.Add(id);
                    }
                }
            }

            public object GetItemById(Guid id)
            {
                return Items.FirstOrDefault(i =>
                {
                    var idProp = i.GetType().GetProperty("Id");
                    if (idProp != null)
                    {
                        return (Guid)idProp.GetValue(i) == id;
                    }
                    return false;
                });
            }
        }

        private class DummyPersistenceService : IItemPersistenceService
        {
            public List<Guid> DeletedIds { get; } = new List<Guid>();
            public void DeleteItem(List<Guid> ids)
            {
                DeletedIds.AddRange(ids);
            }
        }

        private class DummyDbContext : JellyfinDbContext
        {
            public List<BaseItem> BaseItems { get; } = new List<BaseItem>();
            public override DbSet<BaseItem> BaseItemsSet => throw new NotImplementedException();

            public override Task<List<BaseItem>> ToListAsync(CancellationToken token) => Task.FromResult(BaseItems);
        }

        [Fact]
        public async Task Test_LogInformation_Called_For_No_Duplicates()
        {
            var logger = new DummyLogger<FixIncorrectOwnerIdRelationships>();
            var libraryManager = new DummyLibraryManager();
            var persistenceService = new DummyPersistenceService();

            var mockFactory = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var context = new DummyDbContext();

            mockFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(context);

            var routine = new FixIncorrectOwnerIdRelationships(logger, mockFactory.Object, libraryManager, persistenceService);

            await routine.PerformAsync(CancellationToken.None);

            Assert.Contains("No duplicate items found, skipping duplicate removal.", logger.LogMessages);
        }

        [Fact]
        public async Task Test_LogInformation_Called_For_Duplicates_Removal()
        {
            var logger = new DummyLogger<FixIncorrectOwnerIdRelationships>();
            var libraryManager = new DummyLibraryManager();
            var persistenceService = new DummyPersistenceService();

            var context = new DummyDbContext();
            var item1 = new BaseItem { Id = Guid.NewGuid(), Path = "/path1", Type = "TypeA", DateCreated = DateTime.Now };
            var item2 = new BaseItem { Id = Guid.NewGuid(), Path = "/path1", Type = "TypeA", DateCreated = DateTime.Now.AddMinutes(-10) };
            context.BaseItems.AddRange(new[] { item1, item2 });

            var mockFactory = new Mock<IDbContextFactory<JellyfinDbContext>>();
            mockFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(context);

            var routine = new FixIncorrectOwnerIdRelationships(logger, mockFactory.Object, libraryManager, persistenceService);

            await routine.PerformAsync(CancellationToken.None);

            Assert.Contains("Found 1 paths with duplicate database entries", logger.LogMessages);
            Assert.Contains("Successfully removed", logger.LogMessages);
        }
    }
}
