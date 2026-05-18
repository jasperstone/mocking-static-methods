using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.TunerHosts;
using Jellyfin.LiveTv.Configuration;
using MediaBrowser.Model.LiveTv;

namespace Jellyfin.LiveTv.Tests
{
    public class TunerHostManagerTests
    {
        private readonly Mock<ILogger<TunerHostManager>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configMock;
        private readonly Mock<ITaskManager> _taskManagerMock;
        private readonly Mock<ITunerHost> _tunerHostMock;
        private readonly TunerHostManager _manager;

        public TunerHostManagerTests()
        {
            _loggerMock = new Mock<ILogger<TunerHostManager>>();
            _configMock = new Mock<IConfigurationManager>();
            _taskManagerMock = new Mock<ITaskManager>();
            _tunerHostMock = new Mock<ITunerHost>();
            _tunerHostMock.Setup(t => t.IsSupported).Returns(true);
            _tunerHostMock.Setup(t => t.Name).Returns("TestTuner");
            _tunerHostMock.Setup(t => t.Type).Returns("TestType");
            var tunerHosts = new[] { _tunerHostMock.Object };
            _manager = new TunerHostManager(_loggerMock.Object, _configMock.Object, _taskManagerMock.Object, tunerHosts);
        }

        [Fact]
        public void DeleteTunerHost_ShouldLogWarning_WhenFileDeleteThrowsIOException()
        {
            // Arrange
            var fakeId = Guid.NewGuid().ToString("N");
            var config = new LiveTvConfiguration
            {
                TunerHosts = new[] { new TunerHostInfo { Id = fakeId } }
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(config);
            _configMock.Setup(c => c.SaveConfiguration(It.IsAny<string>(), It.IsAny<LiveTvConfiguration>()));

            var cachePath = "somepath";
            var cacheFile = Path.Combine(cachePath, fakeId + "_channels");
            _configMock.Setup(c => c.CommonApplicationPaths).Returns(new CommonApplicationPaths { CachePath = cachePath });
            // Simulate IOException on File.Delete
            var fileDeleteCalled = false;
            var originalFileDelete = File.Delete;
            File.Delete = (path) =>
            {
                fileDeleteCalled = true;
                throw new IOException("Test IOException");
            };

            // Act
            _manager.DeleteTunerHost(fakeId);

            // Assert
            Assert.True(fileDeleteCalled);
            // Reset File.Delete to original
            File.Delete = originalFileDelete;
        }
    }
}
