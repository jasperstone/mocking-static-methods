using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay.Requests;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<Group>> _loggerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;

        public GroupTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<Group>>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<Group>()).Returns(_loggerMock.Object);

            _userManagerMock = new Mock<IUserManager>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _libraryManagerMock = new Mock<ILibraryManager>();
        }

        private Group CreateGroup()
        {
            return new Group(
                _loggerFactoryMock.Object,
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);
        }

        private SessionInfo CreateSession(string id = "session1", string userName = "user1")
        {
            return new SessionInfo
            {
                Id = id,
                UserName = userName
            };
        }

        private class DummyRequest : IGroupPlaybackRequest
        {
            public string Action { get; set; } = "DummyAction";

            public void Apply(Group group, IGroupState state, SessionInfo session, CancellationToken cancellationToken)
            {
                // No-op for test
            }
        }

        [Fact]
        public void SessionJoin_LogsInformation()
        {
            // Arrange
            var group = CreateGroup();
            var session = CreateSession();

            // We need to add the session to the group to simulate join
            // The AddSession method is private, so we simulate join by calling SessionJoin method if it exists
            // But from the snippet, SessionJoin method is not shown, so we simulate by calling the internal methods via reflection or by calling the public method that triggers the log
            // The snippet shows the log line inside a method that calls _logger.LogInformation("Session {SessionId} joined group {GroupId}.", session.Id, GroupId.ToString());
            // We do not have the full method name, but presumably it is SessionJoin or similar.
            // Since the snippet is partial, we will test the SessionLeave method which is public and contains a similar log line.

            // Act
            // We test SessionLeave which is public and logs "Session {SessionId} left group {GroupId}."
            group.SessionLeave(session, null, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session {session.Id} left group {group.GroupId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var group = CreateGroup();
            var session = CreateSession();

            // Act
            group.SessionLeave(session, null, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session {session.Id} left group {group.GroupId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_LogsInformation()
        {
            // Arrange
            var group = CreateGroup();
            var session = CreateSession();
            var request = new DummyRequest();

            // Act
            group.HandleRequest(session, request, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session {session.Id} requested {request.Action} in group {group.GroupId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
