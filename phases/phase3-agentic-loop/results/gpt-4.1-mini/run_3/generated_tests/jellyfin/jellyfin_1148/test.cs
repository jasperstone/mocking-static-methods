using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.TunerHosts;
using Jellyfin.LiveTv.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.LiveTv.Tests.TunerHosts
{
    public class TunerHostManagerTests
    {
        [Fact]
        public void DeleteTunerHost_DeletesCacheFileAndLogsWarningOnIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TunerHostManager>>();
            var configMock = new Mock<IConfigurationManager>();
            var taskManagerMock = new Mock<ITaskManager>();
            var tunerHostMock = new Mock<ITunerHost>();
            tunerHostMock.SetupGet(t => t.IsSupported).Returns(true);
            var tunerHosts = new[] { tunerHostMock.Object };

            var liveTvOptionsMock = new Mock<LiveTvOptions>();
            liveTvOptionsMock.SetupProperty(c => c.TunerHosts, Array.Empty<TunerHostInfo>());

            var commonAppPathsMock = new Mock<ICommonApplicationPaths>();
            var cachePath = Path.GetTempPath();
            commonAppPathsMock.SetupGet(c => c.CachePath).Returns(cachePath);

            configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvOptionsMock.Object);
            configMock.SetupGet(c => c.CommonApplicationPaths).Returns(commonAppPathsMock.Object);

            var manager = new TunerHostManager(loggerMock.Object, configMock.Object, taskManagerMock.Object, tunerHosts);

            // Create a valid GUID string in "N" format
            var guid = Guid.NewGuid();
            var id = guid.ToString("N", CultureInfo.InvariantCulture);

            // Create a dummy file to delete
            var filePath = Path.Combine(cachePath, id + "_channels");
            File.WriteAllText(filePath, "dummy content");

            // Lock the file to cause IOException on delete
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act
                manager.DeleteTunerHost(id);
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting channel cache file for tuner")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            taskManagerMock.Verify(t => t.CancelIfRunningAndQueue<RefreshGuideScheduledTask>(), Times.Once);

            // Cleanup
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
