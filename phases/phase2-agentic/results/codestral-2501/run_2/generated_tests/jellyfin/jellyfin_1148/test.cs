using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly Mock<ILogger<TunerHostManager>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configMock;
        private readonly Mock<ITaskManager> _taskManagerMock;
        private readonly Mock<ITunerHost> _tunerHostMock;
        private readonly TunerHostManager _tunerHostManager;

        public TunerHostManagerTests()
        {
            _loggerMock = new Mock<ILogger<TunerHostManager>>();
            _configMock = new Mock<IConfigurationManager>();
            _taskManagerMock = new Mock<ITaskManager>();
            _tunerHostMock = new Mock<ITunerHost>();

            _tunerHostMock.Setup(t => t.IsSupported).Returns(true);
            _tunerHostMock.Setup(t => t.Name).Returns("Test Tuner Host");
            _tunerHostMock.Setup(t => t.Type).Returns("TestType");

            _tunerHostManager = new TunerHostManager(
                _loggerMock.Object,
                _configMock.Object,
                _taskManagerMock.Object,
                new[] { _tunerHostMock.Object });
        }

        [Fact]
        public void DeleteTunerHost_ValidId_LogsWarningOnIOException()
        {
            // Arrange
            var id = Guid.NewGuid().ToString("N");
            var config = new LiveTvOptions
            {
                TunerHosts = new[] { new TunerHostInfo { Id = id } }
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(config);
            _configMock.Setup(c => c.CommonApplicationPaths.CachePath).Returns(Path.GetTempPath());

            var channelCacheFile = Path.Combine(Path.GetTempPath(), id + "_channels");
            File.WriteAllText(channelCacheFile, "test data");

            // Act
            _tunerHostManager.DeleteTunerHost(id);

            // Assert
            _loggerMock.Verify(
                l => l.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task DiscoverTuners_NewDevicesOnly_ReturnsNewDevices()
        {
            // Arrange
            var config = new LiveTvOptions
            {
                TunerHosts = new[] { new TunerHostInfo { DeviceId = "ExistingDevice" } }
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(config);

            var newDevice = new TunerHostInfo { DeviceId = "NewDevice" };
            _tunerHostMock.Setup(t => t.DiscoverDevices(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TunerHostInfo> { newDevice });

            // Act
            var result = await _tunerHostManager.DiscoverTuners(newDevicesOnly: true).ToListAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(newDevice.DeviceId, result[0].DeviceId);
        }
    }
}
