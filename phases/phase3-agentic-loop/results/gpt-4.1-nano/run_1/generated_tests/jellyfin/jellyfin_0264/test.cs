using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
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

        [Fact]
        public void SessionJoin_LogsInformation()
        {
            // Arrange
            var group = new Group(
                new Mock<ILoggerFactory>().Object,
                _userManagerMock.Object,
                _sessionManagerMock.Object,
                _libraryManagerMock.Object);
            // Replace the logger with our mock to verify calls
            var groupType = typeof(Group);
            var loggerField = groupType.GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(group, _loggerMock.Object);

            var session = new SessionInfo { Id = "session1", UserName = "user1" };
            var cancellationToken = CancellationToken.None;

            // Act
            group.GetType().GetMethod("SessionJoin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(group, new object[] { session, null, cancellationToken });

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Session {SessionId} joined group {GroupId}.", session.Id, It.IsAny<string>()),
                Times.Once);
        }
    }
}
