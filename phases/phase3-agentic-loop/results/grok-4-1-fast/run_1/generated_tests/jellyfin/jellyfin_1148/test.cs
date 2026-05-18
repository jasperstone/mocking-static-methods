using System;
using System.IO;
using Jellyfin.LiveTv.TunerHosts;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests.TunerHosts
{
    public class TunerHostManagerTests
    {
        private readonly Mock<ILogger<TunerHostManager>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configMock;
        private readonly Mock<object> _taskManagerMock;
        private readonly ITunerHost[] _tunerHosts;
        private readonly TunerHostManager _tunerHostManager;

        public TunerHostManagerTests()
        {
            _loggerMock = new Mock<ILogger<TunerHostManager>>();
            _configMock = new Mock<IConfigurationManager>();
            _taskManagerMock = new Mock<object>();
            
            _tunerHosts = Array.Empty<ITunerHost>();
            
            var commonPathsMock = new Mock<ICommonApplicationPaths>();
            commonPathsMock.Setup(p => p.CachePath).Returns("/mock/cache/path");
            _configMock.Setup(c => c.CommonApplicationPaths).Returns(commonPathsMock.Object);
            
            _tunerHostManager = new TunerHostManager(
                _loggerMock.Object,
                _configMock.Object,
                _taskManagerMock.Object,
                _tunerHosts);
        }

        [Fact]
        public void DeleteTunerHost_ValidGuid_NoLogWarningCalled()
        {
            // Arrange
            var tunerId = Guid.NewGuid().ToString("N");

            // Act
            _tunerHostManager.DeleteTunerHost(tunerId);

            // Assert - No LogWarning called since file doesn't exist (no exception thrown)
            _loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void DeleteTunerHost_InvalidGuid_NoLogWarningCalled()
        {
            // Arrange
            var invalidId = "not-a-guid";

            // Act
            _tunerHostManager.DeleteTunerHost(invalidId);

            // Assert - No LogWarning called since Guid parsing fails
            _loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void DeleteTunerHost_NullId_NoLogWarningCalled()
        {
            // Act & Assert
            _tunerHostManager.DeleteTunerHost(null);
            
            _loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void Constructor_SetsTunerHostsProperty()
        {
            // Arrange
            var mockTunerHost = new Mock<ITunerHost>();
            mockTunerHost.Setup(h => h.IsSupported).Returns(true);
            var supportedHosts = new[] { mockTunerHost.Object };
            
            var manager = new TunerHostManager(
                _loggerMock.Object,
                _configMock.Object,
                _taskManagerMock.Object,
                supportedHosts);

            // Assert
            Assert.NotNull(manager.TunerHosts);
            Assert.Single(manager.TunerHosts);
        }
    }
}
