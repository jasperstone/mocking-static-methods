using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        private readonly Mock<ILogger<LibraryMonitor>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerConfigurationManager> _configurationManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;
        private readonly DotIgnoreIgnoreRule _dotIgnoreIgnoreRule;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            // Setup app lifetime to allow Start and Stop registration without error
            _appLifetimeMock.Setup(a => a.ApplicationStarted).Returns(Mock.Of<IApplicationLifetimeToken>());
            _appLifetimeMock.Setup(a => a.ApplicationStopping).Returns(Mock.Of<IApplicationLifetimeToken>());
        }

        [Fact]
        public async Task ReportFileSystemChangeComplete_LogsError_WhenReportFileSystemChangedThrows()
        {
            // Arrange
            var path = "C:\\TestPath";
            var monitor = new TestLibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnoreIgnoreRule);

            // Setup ReportFileSystemChanged to throw
            monitor.ThrowOnReportFileSystemChanged = true;

            // Act
            // Call the async void method and wait a bit to allow it to run
            monitor.ReportFileSystemChangeComplete(path, true);
            await Task.Delay(100); // small delay to allow async void to execute

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in ReportFileSystemChanged")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestLibraryMonitor : LibraryMonitor
        {
            public bool ThrowOnReportFileSystemChanged { get; set; }

            public TestLibraryMonitor(
                ILogger<LibraryMonitor> logger,
                ILibraryManager libraryManager,
                IServerConfigurationManager configurationManager,
                IFileSystem fileSystem,
                IHostApplicationLifetime appLifetime,
                DotIgnoreIgnoreRule dotIgnoreIgnoreRule)
                : base(logger, libraryManager, configurationManager, fileSystem, appLifetime, dotIgnoreIgnoreRule)
            {
            }

            // Override the method to simulate throwing exception
            protected override void ReportFileSystemChanged(string path)
            {
                if (ThrowOnReportFileSystemChanged)
                {
                    throw new InvalidOperationException("Simulated exception");
                }
                base.ReportFileSystemChanged(path);
            }
        }
    }

    // Minimal stub for DotIgnoreIgnoreRule to allow compilation
    public class DotIgnoreIgnoreRule
    {
    }

    // Minimal stub for IApplicationLifetimeToken to allow compilation
    public interface IApplicationLifetimeToken
    {
        void Register(Action callback);
    }
}
