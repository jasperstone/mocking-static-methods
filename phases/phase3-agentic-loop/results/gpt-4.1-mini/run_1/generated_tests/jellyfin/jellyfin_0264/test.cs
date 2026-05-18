using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.SyncPlay;

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

        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var group = new Group(
                _loggerFactoryMock.Object,
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);

            var session = new SessionInfo
            {
                Id = "session1",
                UserName = "user1"
            };

            var request = new LeaveGroupRequest();

            // Add session to group by reflection since AddSession is private
            var participantsField = typeof(Group).GetField("_participants", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var participants = (System.Collections.IDictionary)participantsField.GetValue(group);
            var groupMemberType = typeof(Group).Assembly.GetType("Emby.Server.Implementations.SyncPlay.GroupMember");
            var groupMember = Activator.CreateInstance(groupMemberType, session);
            participants[session.Id] = groupMember;

            // Act
            group.SessionLeave(session, request, CancellationToken.None);

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
    }
}
