using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.SyncPlay;
using Emby.Server.Implementations.SyncPlay;

namespace SyncPlayTests
{
    public class GroupTests
    {
        private readonly Mock<ILogger<Group>> _loggerMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly Mock<ISessionManager> _sessionManagerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;

        public GroupTests()
        {
            _loggerMock = new Mock<ILogger<Group>>();
            _userManagerMock = new Mock<IUserManager>();
            _sessionManagerMock = new Mock<ISessionManager>();
            _libraryManagerMock = new Mock<ILibraryManager>();
        }

        private Group CreateGroup()
        {
            return new Group(
                new Mock<ILoggerFactory>().Object,
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);
        }

        [Fact]
        public void SessionJoin_LogsInformation()
        {
            // Arrange
            var group = CreateGroup();
            var session = new SessionInfo { Id = "session1", UserName = "user1" };
            var cancellationToken = CancellationToken.None;

            // Act
            group.GetType().GetMethod("SessionJoin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(group, new object[] { session, null, cancellationToken });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session {session.Id} joined group")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var group = CreateGroup();
            var session = new SessionInfo { Id = "session2", UserName = "user2" };
            var cancellationToken = CancellationToken.None;

            // Use reflection to invoke private method
            var method = typeof(Group).GetMethod("SessionLeave", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(group, new object[] { session, null, cancellationToken });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session {session.Id} left group")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_LogsInformation()
        {
            // Arrange
            var group = CreateGroup();
            var session = new SessionInfo { Id = "session3" };
            var requestMock = new Mock<IGroupPlaybackRequest>();
            requestMock.Setup(r => r.Action).Returns("Play");
            var request = requestMock.Object;
            var cancellationToken = CancellationToken.None;

            // Use reflection to invoke method
            var method = typeof(Group).GetMethod("HandleRequest", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            method.Invoke(group, new object[] { session, request, cancellationToken });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session {session.Id} requested")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
