#nullable enable

using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Session.Tests
{
    public class SessionManagerTests
    {
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly ListLogger _logger;
        private readonly Mock<IMusicManager> _musicManagerMock;
        private readonly SessionManager _sessionManager;

        public SessionManagerTests()
        {
            _libraryManagerMock = new Mock<ILibraryManager>();
            _musicManagerMock = new Mock<IMusicManager>();
            _logger = new ListLogger();

            // Create mocks only for interfaces we can reference, use NullObject for others where possible
            var userDataManagerMock = new Mock<IUserDataManager>();
            var userManagerMock = new Mock<IUserManager>();

            // Use NullLoggerFactory for dependencies that need ILogger instances internally
            var nullLoggerFactory = NullLoggerFactory.Instance;

            _sessionManager = new SessionManager(
                _logger,
                nullLoggerFactory.CreateLogger<SessionManager>(),
                userDataManagerMock.Object,
                nullLoggerFactory.CreateLogger<SessionManager>(),
                _libraryManagerMock.Object,
                userManagerMock.Object,
                _musicManagerMock.Object,
                nullLoggerFactory.CreateLogger<SessionManager>(),
                nullLoggerFactory.CreateLogger<SessionManager>(),
                nullLoggerFactory.CreateLogger<SessionManager>(),
                nullLoggerFactory.CreateLogger<SessionManager>(),
                nullLoggerFactory.CreateLogger<SessionManager>(),
                new Mock<IHostApplicationLifetime>().Object);
        }

        [Fact]
        public void TranslateItemForInstantMix_NonexistentItemId_LogsError()
        {
            // Arrange
            var nonexistentId = Guid.NewGuid();
            _libraryManagerMock
                .Setup(m => m.GetItemById(nonexistentId))
                .Returns((BaseItem?)null);

            // Act
            var result = _sessionManager.TranslateItemForInstantMix(nonexistentId, null!);

            // Assert
            Assert.Contains($"A nonexistent item Id {nonexistentId}", _logger.Messages[0]);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void TranslateItemForInstantMix_ValidItem_CallsMusicManager()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var user = new Mock<User>().Object;
            var item = new Mock<BaseItem>().Object;
            var expectedResult = new List<BaseItem> { new Mock<BaseItem>().Object() };

            _libraryManagerMock.Setup(m => m.GetItemById(itemId)).Returns(item);
            _musicManagerMock.Setup(m => m.GetInstantMixFromItem(item, user, It.IsAny<DtoOptions>()))
                .Returns(expectedResult);

            // Act
            var result = _sessionManager.TranslateItemForInstantMix(itemId, user);

            // Assert
            Assert.Equal(expectedResult, result);
            _musicManagerMock.Verify(m => m.GetInstantMixFromItem(item, user, It.Is<DtoOptions>(o => !o.EnableImages)), Times.Once);
        }
    }

    public class ListLogger : ILogger
    {
        public List<string> Messages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }
}
