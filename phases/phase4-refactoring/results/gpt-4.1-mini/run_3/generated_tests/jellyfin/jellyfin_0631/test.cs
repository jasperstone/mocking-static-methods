using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class VideoLoggerTests
    {
        [Fact]
        public async Task RefreshMetadataForOwnedVideo_LogsInformation_WhenFileDoesNotExistAndOrphanedVideoFound()
        {
            // Arrange
            var video = new VideoForTest();
            var path = "nonexistentfile";
            var cancellationToken = CancellationToken.None;

            video.SetFileExists(false);

            var orphanedVideo = new Video { OwnerId = video.Id };
            video.SetGetItemByIdResult(orphanedVideo);

            bool logInformationCalled = false;
            video.LoggerLogInformationAction = (message, args) =>
            {
                if (message.Contains("Owned video file no longer exists, removing orphaned item") && args.Length == 1 && (string)args[0] == path)
                {
                    logInformationCalled = true;
                }
            };

            // Act
            await video.RefreshMetadataForOwnedVideo(new MetadataRefreshOptions(), false, path, cancellationToken);

            // Assert
            Assert.True(logInformationCalled);
        }

        private class VideoForTest : Video
        {
            private bool _fileExists = true;
            private Video _getItemByIdResult;

            public Action<string, object[]> LoggerLogInformationAction { get; set; }

            public void SetFileExists(bool exists) => _fileExists = exists;

            public void SetGetItemByIdResult(Video video) => _getItemByIdResult = video;

            // Shadow FileSystem.FileExists call
            protected override bool FileExists(string path) => _fileExists;

            // Shadow LibraryManager.GetItemById call
            protected override Video GetItemById(Guid id) => _getItemByIdResult;

            // Shadow Logger.LogInformation call
            protected override void LogInformation(string message, params object[] args)
            {
                LoggerLogInformationAction?.Invoke(message, args);
            }
        }
    }
}
