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
using MediaBrowser.Common.Extensions;
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
        public void DeleteTunerHost_ValidId_DeletesCacheFile()
        {
            // Arrange
            var tunerId = Guid.NewGuid().ToString("N");
            var config = new LiveTvOptions
            {
                TunerHosts = new[]
                {
                    new TunerHostInfo { Id = tunerId }
                }
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(config);
            _configMock.Setup(c => c.CommonApplicationPaths.CachePath).Returns(Path.GetTempPath());

            // Act
            _tunerHostManager.DeleteTunerHost(tunerId);

            // Assert
            var channelCacheFile = Path.Combine(Path.GetTempPath(), tunerId + "_channels");
            Assert.False(File.Exists(channelCacheFile));
        }

        [Fact]
        public void DeleteTunerHost_InvalidId_LogsWarning()
        {
            // Arrange
            var invalidTunerId = "invalid-id";
            var config = new LiveTvOptions
            {
                TunerHosts = new[]
                {
                    new TunerHostInfo { Id = invalidTunerId }
                }
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(config);
            _configMock.Setup(c => c.CommonApplicationPaths.CachePath).Returns(Path.GetTempPath());

            // Act
            _tunerHostManager.DeleteTunerHost(invalidTunerId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting channel cache file for tuner")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task SaveTunerHost_NewTunerHost_AddsToConfig()
        {
            // Arrange
            var tunerHostInfo = new TunerHostInfo { Type = "TestType" };
            var config = new LiveTvOptions
            {
                TunerHosts = Array.Empty<TunerHostInfo>()
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(config);

            // Act
            var result = await _tunerHostManager.SaveTunerHost(tunerHostInfo);

            // Assert
            Assert.NotNull(result.Id);
            Assert.Single(config.TunerHosts);
            Assert.Equal(tunerHostInfo, config.TunerHosts[0]);
        }

        [Fact]
        public async Task DiscoverTuners_NewDevicesOnly_ReturnsNewDevices()
        {
            // Arrange
            var configuredDeviceIds = new List<string> { "device1" };
            var config = new LiveTvOptions
            {
                TunerHosts = new[]
                {
                    new TunerHostInfo { DeviceId = "device1" }
                }
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(config);
            _tunerHostMock.Setup(t => t.DiscoverDevices(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TunerHostInfo>
                {
                    new TunerHostInfo { DeviceId = "device2" }
                });

            // Act
            var result = await _tunerHostManager.DiscoverTuners(newDevicesOnly: true).ToListAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("device2", result[0].DeviceId);
        }
    }
}
