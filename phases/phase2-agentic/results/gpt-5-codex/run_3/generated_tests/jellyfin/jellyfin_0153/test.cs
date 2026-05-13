using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library.Validators;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Library.Validators
{
    public class PeopleValidatorTests
    {
        [Fact]
        public async Task ValidatePeople_LogsWarningWhenPersonCannotBeRetrieved()
        {
            // Arrange
            var people = new[] { "Missing Person" };

            var libraryManager = new Mock<ILibraryManager>();

            libraryManager
                .Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(people);

            libraryManager
                .Setup(m => m.GetPerson(It.IsAny<string>()))
                .Returns((Person?)null);

            libraryManager
                .Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(Array.Empty<BaseItem>());

            var logger = new TestLogger();
            var fileSystem = new Mock<IFileSystem>();
            var validator = new PeopleValidator(libraryManager.Object, logger, fileSystem.Object);
            var progress = new Mock<IProgress<double>>();

            // Act
            await validator.ValidatePeople(CancellationToken.None, progress.Object);

            // Assert
            var warning = Assert.Single(logger.Entries.Where(entry => entry.LogLevel == LogLevel.Warning));
            Assert.Equal("Failed to get person: Missing Person", warning.Message);
            Assert.True(warning.NamedValues.TryGetValue("Name", out var loggedName));
            Assert.Equal("Missing Person", loggedName);
        }

        private sealed class TestLogger : ILogger
        {
            private sealed class NullScope : IDisposable
            {
                public void Dispose()
                {
                }
            }

            private static readonly IDisposable NullScopeInstance = new NullScope();

            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScopeInstance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;

                var namedValues = state is IEnumerable<KeyValuePair<string, object>> kvps
                    ? kvps.ToDictionary(kv => kv.Key, kv => (object?)kv.Value)
                    : new Dictionary<string, object?>();

                Entries.Add(new LogEntry(logLevel, eventId, message, exception, namedValues));
            }

            public sealed record LogEntry(LogLevel LogLevel, EventId EventId, string Message, Exception? Exception, IReadOnlyDictionary<string, object?> NamedValues);
        }
    }
}
