using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using MediaBrowser.Model.SyncPlay;

namespace SyncPlay.Tests
{
    public class GroupTests
    {
        private readonly Mock<ILogger<Group>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Group _group;

        public GroupTests()
        {
            _loggerMock = new Mock<ILogger<Group>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<Group>()).Returns(_loggerMock.Object);
            _userManagerMock = new Mock<IUserManager>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _libraryManagerMock = new Mock<ILibraryManager>();

            _group = new Group(
                _loggerFactoryMock.Object,
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);
        }

        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var session = new SessionInfo { Id = "session1", UserName = "user1" };
            var mockLogger = new Mock<ILogger<Group>>();
            var group = new Group(
                _loggerFactoryMock.Object,
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);
            // Use reflection to set the private _logger field
            var loggerField = typeof(Group).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(group, mockLogger.Object);

            // Act
            group.SessionLeave(session, null, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation("Session {SessionId} left group {GroupId}.", session.Id, It.IsAny<string>()),
                Times.Once);
        }
    }
}
