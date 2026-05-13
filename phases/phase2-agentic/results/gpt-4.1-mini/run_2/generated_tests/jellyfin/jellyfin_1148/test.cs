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
                    new TunerHostInfo { Id = Guid.NewGuid().ToString("N") }
                }
            };

            configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvConfig);
            configMock.SetupGet(c => c.CommonApplicationPaths).Returns(new CommonApplicationPaths { CachePath = Path.GetTempPath() });

            var manager = new TunerHostManager(loggerMock.Object, configMock.Object, taskManagerMock.Object, new[] { tunerHostMock.Object });

            // Create a valid GUID id for the tuner host to delete
            var tunerId = liveTvConfig.TunerHosts[0].Id;

            // Setup File.Delete to throw IOException when called with the expected path
            var expectedFilePath = Path.Combine(configMock.Object.CommonApplicationPaths.CachePath, tunerId + "_channels");

            // Use a shim for File.Delete by replacing it with a delegate via reflection or use a wrapper interface
            // Since we cannot replace File.Delete easily, we simulate by creating a file and locking it to cause IOException

            // Create and lock the file to cause IOException on delete
            using (var fs = new FileStream(expectedFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                // Act
                manager.DeleteTunerHost(tunerId);

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

            // Cleanup the file after test
            try
            {
                File.Delete(expectedFilePath);
            }
            catch
            {
                // ignore cleanup errors
            }
        }
    }
}
