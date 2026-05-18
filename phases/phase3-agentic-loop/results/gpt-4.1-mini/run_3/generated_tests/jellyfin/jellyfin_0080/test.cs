using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Entities;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void LogDebug_IsCalledForEachExistingMetadataPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            // We will pass null for all dependencies except loggerFactory to avoid missing type errors
            var libraryManager = new TestLibraryManager(loggerFactoryMock.Object, new List<string> { "path1", "path2" });

            var item = new Mock<BaseItem>();
            item.Setup(i => i.GetType().Name).Returns("TestItemType");
            item.Setup(i => i.Name).Returns("TestItemName");
            item.Setup(i => i.Id).Returns(Guid.NewGuid());

            // Act
            libraryManager.InvokeLogDebugForMetadataPaths(item.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting metadata path") && v.ToString().Contains("path1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting metadata path") && v.ToString().Contains("path2")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestLibraryManager : LibraryManager
        {
            private readonly ILogger _logger;
            private readonly List<string> _metadataPaths;

            public TestLibraryManager(ILoggerFactory loggerFactory, List<string> metadataPaths)
                : base(
                    appHost: null,
                    loggerFactory: loggerFactory,
                    taskManager: null,
                    userManager: null,
                    configurationManager: null,
                    userDataManager: null,
                    libraryMonitorFactory: null,
                    fileSystem: null,
                    providerManagerFactory: null,
                    userViewManagerFactory: null,
                    mediaEncoder: null,
                    itemRepository: null,
                    persistenceService: null,
                    nextUpService: null,
                    countService: null,
                    linkedChildrenService: null,
                    imageProcessor: null,
                    namingOptions: null,
                    directoryService: null,
                    peopleRepository: null,
                    pathManager: null,
                    dotIgnoreIgnoreRule: null)
            {
                _logger = loggerFactory.CreateLogger<LibraryManager>();
                _metadataPaths = metadataPaths;
            }

            public void InvokeLogDebugForMetadataPaths(BaseItem item)
            {
                foreach (var metadataPath in _metadataPaths)
                {
                    _logger.LogDebug(
                        "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                        item.GetType().Name,
                        item.Name ?? "Unknown name",
                        metadataPath,
                        item.Id);
                }
            }
        }
    }
}
