using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class VideoLoggerTests
    {
        private class TestLogger : ILogger
        {
            public List<string> Logs = new();

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (formatter != null)
                {
                    var message = formatter(state, exception);
                    Logs.Add(message);
                }
            }
        }

        private class TestVideo : Video
        {
            public ILogger TestLogger { get; }

            public TestVideo(ILogger logger)
            {
                TestLogger = logger;
            }

            protected override ILogger Logger => TestLogger;

            // Expose the private RefreshMetadataForOwnedVideo method for testing
            public Task RefreshMetadataForOwnedVideoPublic(MetadataRefreshOptions options, bool copyTitleMetadata, string path, CancellationToken cancellationToken)
            {
                return base.RefreshMetadataForOwnedVideo(options, copyTitleMetadata, path, cancellationToken);
            }

            // Provide minimal stubs for dependencies to allow method to run without exceptions
            public override ILibraryManager LibraryManager { get; set; }
            public override IFileSystem FileSystem { get; set; }
        }

        // Minimal stub for MetadataRefreshOptions to allow compilation
        public class MetadataRefreshOptions
        {
            public MetadataRefreshOptions() { }
            public MetadataRefreshOptions(MetadataRefreshOptions other) { }
            public object SearchResult { get; set; }
        }

        // Minimal stub for ILibraryManager
        public interface ILibraryManager
        {
            Guid GetNewItemId(string path, Type itemType);
            BaseItem GetItemById(Guid id);
            void DeleteItem(BaseItem item, DeleteOptions options);
        }

        // Minimal stub for IFileSystem
        public interface IFileSystem
        {
            bool FileExists(string path);
        }

        // Minimal stub for DeleteOptions
        public class DeleteOptions
        {
            public bool DeleteFileLocation { get; set; }
        }

        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformationWhenFileDoesNotExist()
        {
            // Arrange
            var path = "nonexistentfile";
            var cancellationToken = CancellationToken.None;
            var options = new MetadataRefreshOptions();

            var logger = new TestLogger();

            var orphanedVideo = new Video { OwnerId = Guid.NewGuid() };

            var libraryManager = new TestLibraryManager(orphanedVideo);
            var fileSystem = new TestFileSystem();

            var video = new TestVideo(logger)
            {
                LibraryManager = libraryManager,
                FileSystem = fileSystem,
                Id = orphanedVideo.OwnerId
            };

            // Act
            await video.RefreshMetadataForOwnedVideoPublic(options, false, path, cancellationToken);

            // Assert
            Assert.Contains(logger.Logs, log => log.Contains("Owned video file no longer exists, removing orphaned item:"));
            Assert.True(libraryManager.DeleteCalled);
        }

        private class TestLibraryManager : ILibraryManager
        {
            private readonly BaseItem _orphanedVideo;
            public bool DeleteCalled { get; private set; }

            public TestLibraryManager(BaseItem orphanedVideo)
            {
                _orphanedVideo = orphanedVideo;
            }

            public Guid GetNewItemId(string path, Type itemType)
            {
                return Guid.NewGuid();
            }

            public BaseItem GetItemById(Guid id)
            {
                return _orphanedVideo;
            }

            public void DeleteItem(BaseItem item, DeleteOptions options)
            {
                DeleteCalled = true;
            }
        }

        private class TestFileSystem : IFileSystem
        {
            public bool FileExists(string path) => false;
        }
    }
}
