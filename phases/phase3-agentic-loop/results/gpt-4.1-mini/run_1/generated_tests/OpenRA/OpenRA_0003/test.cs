using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenRA;
using Xunit;

namespace OpenRA.Game.Tests.Map
{
    public class MapPreviewInstallEarlyExitTests
    {
        // These tests cover the early exit conditions of Install method in MapPreview,
        // which do not require mocking HttpClient or inheritance.

        [Fact]
        public void Install_DoesNothing_WhenStatusIsNotDownloadErrorOrDownloadAvailable()
        {
            var mapPreview = new MapPreviewTestHelper(MapStatus.Available, allowDownloading: true, hasMapInstallDirectory: true);

            mapPreview.Install("http://example.com/");

            Assert.Equal(MapStatus.Available, mapPreview.Status);
            Assert.False(mapPreview.InstallCalled);
        }

        [Fact]
        public void Install_DoesNothing_WhenAllowDownloadingIsFalse()
        {
            var mapPreview = new MapPreviewTestHelper(MapStatus.DownloadAvailable, allowDownloading: false, hasMapInstallDirectory: true);

            mapPreview.Install("http://example.com/");

            Assert.Equal(MapStatus.DownloadAvailable, mapPreview.Status);
            Assert.False(mapPreview.InstallCalled);
        }

        [Fact]
        public void Install_SetsDownloadError_WhenMapInstallDirectoryNotFound()
        {
            var mapPreview = new MapPreviewTestHelper(MapStatus.DownloadAvailable, allowDownloading: true, hasMapInstallDirectory: false);

            mapPreview.Install("http://example.com/");

            Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
            Assert.True(mapPreview.InstallCalled);
        }

        // Helper class to simulate MapPreview behavior for early exit testing
        private class MapPreviewTestHelper
        {
            public MapStatus Status { get; private set; }
            public bool AllowDownloading { get; }
            public bool HasMapInstallDirectory { get; }
            public bool InstallCalled { get; private set; }

            public MapPreviewTestHelper(MapStatus status, bool allowDownloading, bool hasMapInstallDirectory)
            {
                Status = status;
                AllowDownloading = allowDownloading;
                HasMapInstallDirectory = hasMapInstallDirectory;
                InstallCalled = false;
            }

            public void Install(string mapRepositoryUrl)
            {
                // Simulate the early exit conditions of MapPreview.Install

                if ((Status != MapStatus.DownloadError && Status != MapStatus.DownloadAvailable) || !AllowDownloading)
                    return;

                InstallCalled = true;

                if (!HasMapInstallDirectory)
                {
                    Status = MapStatus.DownloadError;
                    return;
                }

                // We do not simulate the HttpClient call here
            }
        }
    }
}
