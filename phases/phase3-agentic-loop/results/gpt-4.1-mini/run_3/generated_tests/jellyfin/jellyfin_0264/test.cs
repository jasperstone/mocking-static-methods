using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<Group>> _loggerMock;

        public GroupTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<Group>>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<Group>()).Returns(_loggerMock.Object);
        }

        private Group CreateGroup()
        {
            // We pass null for unused dependencies to avoid missing type errors
            return new Group(
                _loggerFactoryMock.Object,
                userManager: null,
                sessionManager: null,
                libraryManager: null);
        }

        private SessionInfo CreateSession(string id = "session1", string userName = "user1")
        {
            return new SessionInfo
            {
                Id = id,
                UserName = userName
            };
        }

        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var group = CreateGroup();
            var session = CreateSession();

            // Add session to private _participants dictionary via reflection
            var participantsField = typeof(Group).GetField("_participants", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var participants = (System.Collections.IDictionary)participantsField.GetValue(group);
            participants.Add(session.Id, new GroupMember(session));

            var cancellationToken = CancellationToken.None;

            // Act
            group.SessionLeave(session, new LeaveGroupRequest(), cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session {session.Id} left group {group.GroupId}.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void SessionJoin_LogsInformation()
        {
            // Arrange
            var group = CreateGroup();
            var session = CreateSession();
            var cancellationToken = CancellationToken.None;

            // Use reflection to get SessionJoin method and invoke it
            var sessionJoinMethod = typeof(Group).GetMethod("SessionJoin", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(sessionJoinMethod);

            // Act
            sessionJoinMethod.Invoke(group, new object[] { session, cancellationToken });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session {session.Id} joined group {group.GroupId}.")),
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

            var requestMock = new Mock<dynamic>();
            requestMock.SetupGet(r => r.Action).Returns("DummyAction");
            requestMock.Setup(r => r.Apply(It.IsAny<Group>(), It.IsAny<object>(), It.IsAny<SessionInfo>(), It.IsAny<CancellationToken>()));

            var cancellationToken = CancellationToken.None;

            // Act
            group.HandleRequest(session, requestMock.Object, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session {session.Id} requested DummyAction in group {group.GroupId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal LeaveGroupRequest stub to satisfy parameter
    public class LeaveGroupRequest { }
}
