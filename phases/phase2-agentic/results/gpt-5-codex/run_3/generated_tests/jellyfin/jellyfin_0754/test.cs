using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jellyfin.Tests.MediaEncoding.Encoder
{
    public class EncoderValidatorTests
    {
        [Fact]
        public void GetCodecs_WhenProcessThrows_LogsErrorAndReturnsEmpty()
        {
            var logger = new TestLogger();
            var nonexistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var validator = new EncoderValidator(logger, nonexistentPath);

            var codecType = typeof(EncoderValidator).GetNestedType("Codec", BindingFlags.NonPublic);
            Assert.NotNull(codecType);

            var encoderValue = Enum.Parse(codecType!, "Encoder");
            var method = typeof(EncoderValidator).GetMethod("GetCodecs", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);

            var result = method!.Invoke(validator, new[] { encoderValue }) as IEnumerable<string>;
            Assert.NotNull(result);
            Assert.Empty(result!);

            var logEntry = Assert.Single(logger.Entries.Where(entry => entry.Level == LogLevel.Error));
            Assert.NotNull(logEntry.Exception);
            Assert.Equal("Error detecting available encoders", logEntry.Message);
        }

        private sealed class TestLogger : ILogger
        {
            private sealed class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();
                public void Dispose()
                {
                }
            }

            public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : string.Empty;
                Entries.Add((logLevel, message, exception));
            }
        }
    }
}
