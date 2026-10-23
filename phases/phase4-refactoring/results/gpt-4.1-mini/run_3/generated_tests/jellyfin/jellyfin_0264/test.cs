using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.SyncPlay;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    // Minimal stub for SessionInfo to allow compilation
    public class SessionInfo
    {
        public string Id { get; set; }
        public string UserName { get; set; }
    }

    // Minimal stub for LeaveGroupRequest to allow compilation
    public class LeaveGroupRequest
    {
    }

    // Minimal interface stubs for dependencies
    public interface IUserManager
    {
    }

    public interface ISessionManager
    {
    }

    public interface ILibraryManager
    {
    }

    public class GroupTests
    {
        private class TestGroup : Group
        {
            public TestGroup(
                ILoggerFactory loggerFactory,
                IUserManager userManager,
                ISessionManager sessionManager,
                ILibraryManager libraryManager,
                ILogger<Group> logger)
                : base(loggerFactory, userManager, sessionManager, libraryManager)
            {
                // Override the _logger field via reflection since it's private readonly
                var loggerField = typeof(Group).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                loggerField.SetValue(this, logger);
            }
        }

        [Fact]
        public void SessionLeave_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<Group>()).Returns(loggerMock.Object);

            var userManagerMock = new Mock<IUserManager>();
            var sessionManagerMock = new Mock<ISessionManager>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var group = new TestGroup(loggerFactoryMock.Object, userManagerMock.Object, sessionManagerMock.Object, libraryManagerMock.Object, loggerMock.Object);

            var session = new SessionInfo
            {
                Id = "session1",
                UserName = "user1"
            };

            var request = new LeaveGroupRequest();

            var cancellationToken = CancellationToken.None;

            // Add session to group so RemoveSession can remove it
            var addSessionMethod = typeof(Group).GetMethod("AddSession", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            addSessionMethod.Invoke(group, new object[] { session });

            // Act
            group.SessionLeave(session, request, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Session {session.Id} left group {group.GroupId}."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
