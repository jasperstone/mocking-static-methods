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
using System.Threading.Tasks;

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

            var liveTvConfig = new LiveTvConfiguration
            {
                TunerHosts = Array.Empty<TunerHostInfo>()
            };

            var commonAppPathsMock = new Mock<ICommonApplicationPaths>();
            var cachePath = Path.GetTempPath();
            commonAppPathsMock.SetupGet(c => c.CachePath).Returns(cachePath);

            configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvConfig);
            configMock.SetupGet(c => c.CommonApplicationPaths).Returns(commonAppPathsMock.Object);

            var manager = new TunerHostManager(
                loggerMock.Object,
                configMock.Object,
                taskManagerMock.Object,
                new[] { tunerHostMock.Object });

            // Create a valid GUID string in "N" format
            var guid = Guid.NewGuid();
            var id = guid.ToString("N", CultureInfo.InvariantCulture);

            // Create a file path that will throw IOException on delete
            var filePath = Path.Combine(cachePath, id + "_channels");

            // Create the file so it exists
            File.WriteAllText(filePath, "test");

            // Lock the file to cause IOException on delete
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act
                manager.DeleteTunerHost(id);

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting channel cache file for tuner")),
                        It.IsAny<IOException>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            // Cleanup
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // ignore cleanup errors
            }
        }
    }
}
