using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library.Validators;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.Library.Validators;

public class PeopleValidatorTests
{
    [Fact]
    public async Task ValidatePeople_LogsWarningWhenPersonMissing()
    {
        // Arrange
        var libraryManager = new Mock<ILibraryManager>();
        var logger = new TestLogger();
        var fileSystem = Mock.Of<IFileSystem>();
        var progress = new Progress<double>(_ => { });

        var peopleNames = new List<string> { "John" };

        libraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
            .Returns(peopleNames);
        libraryManager.Setup(m => m.GetPerson("John"))
            .Returns((BaseItem?)null);
        libraryManager.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
            .Returns(new List<BaseItem>());

        var validator = new PeopleValidator(libraryManager.Object, logger, fileSystem);

        // Act
        await validator.ValidatePeople(CancellationToken.None, progress);

        // Assert
        var warningEntry = Assert.Single(logger.Entries.Where(entry => entry.Level == LogLevel.Warning));
        Assert.Equal("Failed to get person: John", warningEntry.Message);

        var stateDictionary = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(warningEntry.State);
        var nameEntry = Assert.Single(stateDictionary, kvp => kvp.Key == "Name");
        Assert.Equal("John", nameEntry.Value);
    }

    private sealed class TestLogger : ILogger
    {
        public List<LogEntry> Entries { get; } = new();

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            Entries.Add(new LogEntry(
                logLevel,
                eventId,
                formatter(state, exception),
                exception,
                state!));
        }

        public readonly record struct LogEntry(
            LogLevel Level,
            EventId EventId,
            string Message,
            Exception? Exception,
            object State);

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
