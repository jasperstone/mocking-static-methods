using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class RecoveryLoggerTests
    {
        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenRecoveringNonEmptyLog()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            // Capture the log message using a custom formatter
            bool messageLogged = false;
            mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception?, Func<object, Exception?, string>>((level, eventId, state, ex, formatter) =>
                {
                    if (level == LogLevel.Information)
                    {
                        var message = formatter(state, ex);
                        if (message.Contains("Recovery called on non-empty log - resetting to empty state first"))
                        {
                            messageLogged = true;
                        }
                    }
                });

            // Create real TsavoriteKV instance with mocked dependencies via reflection
            var tsavoriteType = typeof(TsavoriteKV<long, long, EmptyDefaultFunctions, Allocator<long, long, EmptyDefaultFunctions>>);
            var instance = Activator.CreateInstance(tsavoriteType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { }, null);

            // Set logger via reflection
            var loggerField = typeof(TsavoriteBase).GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField?.SetValue(instance, mockLogger.Object);

            // Mock hlogBase and hlog via reflection and setup the condition
            var hlogBaseField = tsavoriteType.GetField("hlogBase", BindingFlags.NonPublic | BindingFlags.Instance);
            var hlogField = tsavoriteType.GetField("hlog", BindingFlags.NonPublic | BindingFlags.Instance);

            var mockHlogBase = new Mock<object>().Object;
            var mockHlog = new Mock<object>().Object;
            hlogBaseField?.SetValue(instance, mockHlogBase);
            hlogField?.SetValue(instance, mockHlog);

            // Use reflection to call methods that return values for the condition
            var getTailAddressMethod = tsavoriteType.GetMethod("hlogBase_GetTailAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            var getFirstValidMethod = tsavoriteType.GetMethod("hlog_GetFirstValidLogicalAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            
            getTailAddressMethod?.Invoke(instance, null); // This will be > first valid
            getFirstValidMethod?.Invoke(instance, new object[] { 0L });

            // Act
            var internalRecoverMethod = tsavoriteType.GetMethod("InternalRecoverAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task<long>)internalRecoverMethod!.Invoke(instance, new object[] { default!, default!, 0, false, 0L, CancellationToken.None })!;

            // Assert
            Assert.True(messageLogged);
            mockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
        }
    }
}
