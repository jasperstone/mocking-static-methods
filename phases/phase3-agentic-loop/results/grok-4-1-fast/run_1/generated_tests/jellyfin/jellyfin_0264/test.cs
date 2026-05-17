using System;
using System.Threading;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.SyncPlay;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        private readonly Mock<ILogger<Group>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;

        public GroupTests()
        {
            _loggerMock = new Mock<ILogger<Group>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<Group>()).Returns(_loggerMock.Object);
            _userManagerMock = new Mock<IUserManager>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _libraryManagerMock = new Mock<ILibraryManager>();
        }

        [Fact]
        public void SessionLeave_LogsInformationMessage()
        {
            // Arrange
            var group = new Group(_loggerFactoryMock.Object, _userManagerMock.Object, _sessionManagerMock.Object, _libraryManagerMock.Object);
            var session = new SessionInfo(null, null)
            {
                Id = "test-session-id"
            };

            // Create mock LeaveGroupRequest
            var requestMock = new Mock<LeaveGroupRequest>();
            var cancellationToken = new CancellationToken();

            // Act
            group.SessionLeave(session, requestMock.Object, cancellationToken);

            // Assert - specifically targeting line 323 LogInformation call
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session test-session-id left group") && v.ToString().Contains(group.GroupId.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void SessionJoin_LogsInformationMessage()
        {
            // Arrange
            var group = new Group(_loggerFactoryMock.Object, _userManagerMock.Object, _sessionManagerMock.Object, _libraryManagerMock.Object);
            var session = new SessionInfo(null, null)
            {
                Id = "test-session-id"
            };

            // Create mock JoinGroupRequest
            var requestMock = new Mock<JoinGroupRequest>();
            var cancellationToken = new CancellationToken();

            // Act
            group.SessionJoin(session, requestMock.Object, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session test-session-id joined group") && v.ToString().Contains(group.GroupId.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_LogsInformationMessage()
        {
            // Arrange
            var group = new Group(_loggerFactoryMock.Object, _userManagerMock.Object, _sessionManagerMock.Object, _libraryManagerMock.Object);
            var session = new SessionInfo(null, null)
            {
                Id = "test-session-id"
            };

            // Create mock IGroupPlaybackRequest
            var requestMock = new Mock<IGroupPlaybackRequest>();
            requestMock.Setup(r => r.Action).Returns("TestAction");
            var cancellationToken = new CancellationToken();

            // Act
            group.HandleRequest(session, requestMock.Object, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Session test-session-id requested TestAction in group") && v.ToString().Contains(group.GroupId.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
