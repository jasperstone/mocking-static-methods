using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.Configuration;
using Jellyfin.LiveTv.Guide;
using Jellyfin.LiveTv.TunerHosts;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests.TunerHosts
{
    public class TunerHostManagerTests
    {
        [Fact]
        public void DeleteTunerHost_WhenIOExceptionThrown_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TunerHostManager>>();
            var configMock = new Mock<IConfigurationManager>();
            var taskManagerMock = new Mock<ITaskManager>();
            var tunerHosts = new List<ITunerHost>();

            var tunerHostManager = new TunerHostManager(loggerMock.Object, configMock.Object, taskManagerMock.Object, tunerHosts);

            var tunerId = Guid.NewGuid().ToString("N");
            var channelCacheFile = Path.Combine("cache", tunerId + "_channels");

            configMock.Setup(c => c.CommonApplicationPaths.CachePath).Returns("cache");
            configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(new LiveTvConfiguration());

            // Mock the File.Delete method to throw an IOException
            var fileSystemMock = new Mock<FileSystem>();
            fileSystemMock.Setup(fs => fs.Delete(channelCacheFile)).Throws(new IOException());

            // Act
            tunerHostManager.DeleteTunerHost(tunerId);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Mock FileSystem class to simulate file operations
    public class FileSystem
    {
        public virtual void Delete(string path)
        {
            File.Delete(path);
        }
    }
}
