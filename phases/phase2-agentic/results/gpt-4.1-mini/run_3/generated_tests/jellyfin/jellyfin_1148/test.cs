using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.TunerHosts;
using Jellyfin.LiveTv.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Tasks;
using System.Threading.Tasks;

namespace Jellyfin.LiveTv.Tests.TunerHosts
{
    public class TunerHostManagerTests
    {
        [Fact]
        public void DeleteTunerHost_LogsWarningOnIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TunerHostManager>>();
            var configMock = new Mock<IConfigurationManager>();
            var taskManagerMock = new Mock<ITaskManager>();
            var tunerHostMock = new Mock<ITunerHost>();
            tunerHostMock.SetupGet(t => t.IsSupported).Returns(true);

            var liveTvConfig = new LiveTvConfiguration
            {
                TunerHosts = new[]
                {
                    new TunerHostInfo { Id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture) }
                }
            };

            configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvConfig);
            configMock.SetupGet(c => c.CommonApplicationPaths).Returns(new CommonApplicationPaths { CachePath = Path.GetTempPath() });

            var manager = new TunerHostManager(loggerMock.Object, configMock.Object, taskManagerMock.Object, new[] { tunerHostMock.Object });

            // Create a valid GUID string for id
            var id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

            // Setup File.Delete to throw IOException by using a wrapper or by substituting File.Delete
            // Since File.Delete is static, we cannot mock it directly.
            // Instead, we create a temp file and make it read-only to cause IOException on delete.
            var filePath = Path.Combine(configMock.Object.CommonApplicationPaths.CachePath, id + "_channels");
            File.WriteAllText(filePath, "test");
            var fileInfo = new FileInfo(filePath);
            fileInfo.IsReadOnly = true;

            try
            {
                // Act
                manager.DeleteTunerHost(id);
            }
            finally
            {
                // Cleanup: remove read-only and delete file
                fileInfo.IsReadOnly = false;
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
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
        }
    }
}
