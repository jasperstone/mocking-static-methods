using System;
using System.IO;
using System.Linq;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Devices
{
    public class DeviceIdTests
    {
        [Fact]
        public void Value_LogsErrorWhenCacheReadThrowsUnexpectedException()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                Directory.CreateDirectory(Path.Combine(tempDir, "device.txt"));

                var appPathsMock = new Mock<IApplicationPaths>();
                appPathsMock.SetupGet(p => p.DataPath).Returns(tempDir);

                var loggerMock = new Mock<ILogger<DeviceId>>();

                var deviceId = new DeviceId(appPathsMock.Object, loggerMock.Object);

                var value = deviceId.Value;

                Assert.False(string.IsNullOrWhiteSpace(value));

                var logEntries = loggerMock.Invocations
                    .Where(invocation =>
                        invocation.Method.Name == nameof(ILogger.Log) &&
                        invocation.Arguments.Count >= 4 &&
                        invocation.Arguments[2]?.ToString() == "Error reading file")
                    .ToList();

                var entry = Assert.Single(logEntries);
                Assert.IsType<UnauthorizedAccessException>(entry.Arguments[3]);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
