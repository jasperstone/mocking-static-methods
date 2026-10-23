using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;

namespace Emby.Server.Implementations.Library.Tests
{
    // Minimal stubs for missing types to allow compilation
    public class ItemUpdateType { }
    public class ItemUpdateOptions { }

    public class LibraryManagerTests
    {
        private class TestLibraryManager : LibraryManager
        {
            private readonly IEnumerable<string> _metadataPaths;

            public TestLibraryManager(
                ILoggerFactory loggerFactory,
                IEnumerable<string> metadataPaths)
                : base(
                    appHost: null!,
                    loggerFactory: loggerFactory,
                    taskManager: null!,
                    userManager: null!,
                    configurationManager: null!,
                    userDataManager: null!,
                    libraryMonitorFactory: new Lazy<ILibraryMonitor>(() => null!),
                    fileSystem: null!,
                    providerManagerFactory: new Lazy<IProviderManager>(() => null!),
                    userViewManagerFactory: new Lazy<IUserViewManager>(() => null!),
                    mediaEncoder: null!,
                    itemRepository: null!,
                    persistenceService: null!,
                    nextUpService: null!,
                    countService: null!,
                    linkedChildrenService: null!,
                    imageProcessor: null!,
                    namingOptions: null!,
                    directoryService: null!,
                    peopleRepository: null!,
                    pathManager: null!,
                    dotIgnoreIgnoreRule: null!)
            {
                _metadataPaths = metadataPaths;
            }

            // Shadow the private GetMetadataPaths method with a public one for testing
            public new IEnumerable<string> GetMetadataPaths(BaseItem item, IEnumerable<BaseItem> children)
            {
                return _metadataPaths;
            }

            // Expose DeleteItem for testing
            public new void DeleteItem(BaseItem item, ItemUpdateType updateType, ItemUpdateOptions options)
            {
                base.DeleteItem(item, updateType, options);
            }
        }

        [Fact]
        public void DeleteItem_LogsDebugForEachMetadataPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var metadataPaths = new[] { "path1", "path2" };

            var libraryManager = new TestLibraryManager(loggerFactoryMock.Object, metadataPaths);

            var testItem = new BaseItem
            {
                Id = Guid.NewGuid(),
                Name = "TestItem",
                IsFolder = false
            };

            // Act
            libraryManager.DeleteItem(testItem, new ItemUpdateType(), new ItemUpdateOptions());

            // Assert
            foreach (var path in metadataPaths)
            {
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Debug,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleting metadata path") && v.ToString()!.Contains(path)),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
        }
    }
}
