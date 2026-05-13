using System;
using System.IO;
using System.Reflection;
using Emby.Server.Implementations.Devices;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Devices
{
    public class DeviceIdTests
    {
        [Fact]
        public void GetCachedId_LogsErrorWhenExceptionThrownReadingFile()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"DeviceIdTests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);
            var cachePath = Path.Combine(tempDir, "device.txt");

            var appPathsMock = new Mock<IApplicationPaths>();
            appPathsMock.SetupGet(p => p.DataPath).Returns(tempDir);

            var loggerMock = new Mock<ILogger<DeviceId>>();

            var deviceId = new DeviceId(appPathsMock.Object, loggerMock.Object);

            FileStream? lockedStream = null;

            try
            {
                lockedStream = new FileStream(cachePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

                var methodInfo = typeof(DeviceId).GetMethod("GetCachedId", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.NotNull(methodInfo);

                var result = methodInfo!.Invoke(deviceId, null);

                Assert.Null(result);

                loggerMock.Verify(
                    m => m.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((state, _) => state.ToString() == "Error reading file"),
                        It.Is<Exception>(ex => ex is IOException),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                lockedStream?.Dispose();
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
