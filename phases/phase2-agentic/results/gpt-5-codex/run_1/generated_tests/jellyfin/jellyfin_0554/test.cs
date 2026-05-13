using System;
using System.Collections.Generic;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_WhenUserDatabaseMissing_LogsWarning()
        {
            var tempDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            var paths = new Mock<IServerApplicationPaths>();
            paths.SetupGet(p => p.DataPath).Returns(tempDataPath);

            var logger = new TestLogger<MigrateUserDb>();

            var sut = new MigrateUserDb(logger, paths.Object, default!, default!);

            sut.Perform();

            var expectedPath = Path.Combine(tempDataPath, "users.db");

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Equal($"{expectedPath} doesn't exist, nothing to migrate", entry.Message);
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            private readonly List<(LogLevel Level, string Message, Exception? Exception)> _entries = new();

            public IReadOnlyList<(LogLevel Level, string Message, Exception? Exception)> Entries => _entries;

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (formatter is null)
                {
                    throw new ArgumentNullException(nameof(formatter));
                }

                var message = formatter(state, exception);
                _entries.Add((logLevel, message, exception));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
