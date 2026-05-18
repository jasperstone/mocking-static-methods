using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.TunerHosts;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.LiveTv.TunerHosts
{
    public class TunerHostManagerTests
    {
        private readonly Mock<ILogger<TunerHostManager>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configMock;
        private readonly Mock<ITaskManager> _taskManagerMock;
        private readonly Mock<ITunerHost> _tunerHostMock;
        private readonly TunerHostManager _manager;
        private readonly string _cachePath = Path.Combine(Path.GetTempPath(), "testcache");

        public TunerHostManagerTests()
        {
            _loggerMock = new Mock<ILogger<TunerHostManager>>();
            _configMock = new Mock<IConfigurationManager>();
            _taskManagerMock = new Mock<ITaskManager>();
            _tunerHostMock = new Mock<ITunerHost>();
            _tunerHostMock.Setup(t => t.IsSupported).Returns(true);
            _tunerHostMock.Setup(t => t.Name).Returns("TestHost");
            _tunerHostMock.Setup(t => t.Type).Returns("TestType");
            _tunerHostMock.Setup(t => t.DiscoverDevices(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { new TunerHostInfo { DeviceId = "device1", Url = "http://url" } });
            var tunerHosts = new[] { _tunerHostMock.Object };
            _manager = new TunerHostManager(_loggerMock.Object, _configMock.Object, _taskManagerMock.Object, tunerHosts);
            Directory.CreateDirectory(_cachePath);
            _configMock.Setup(c => c.CommonApplicationPaths.CachePath).Returns(_cachePath);
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(new LiveTvConfiguration
            {
                TunerHosts = Array.Empty<TunerHostInfo>()
            });
        }

        [Fact]
        public async Task DeleteTunerHost_ShouldLogWarning_WhenFileDeleteThrowsIOException()
        {
            // Arrange
            var id = Guid.NewGuid().ToString("N");
            var config = new LiveTvConfiguration
            {
                TunerHosts = new[] { new TunerHostInfo { Id = id } }
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(config);
            var filePath = Path.Combine(_cachePath, id + "_channels");
            File.WriteAllText(filePath, "test");
            var exception = new IOException("Test IO exception");
            // Act
            await _manager.DeleteTunerHost(id);
            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<IOException>(ex => ex.Message == exception.Message), "Error deleting channel cache file for tuner {TunerId}", id), Times.Once);
            // Cleanup
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
