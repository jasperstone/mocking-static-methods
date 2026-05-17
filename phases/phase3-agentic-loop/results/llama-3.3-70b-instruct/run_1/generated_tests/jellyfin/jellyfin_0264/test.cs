using Emby.Server.Implementations.SyncPlay;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.SyncPlay.Tests
{
    public class GroupTests
    {
        [Fact]
        public void SessionLeave_LogsSessionLeave()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var sessionManagerMock = new Mock<ISessionManager>();
            var group = new Group(
                loggerFactoryMock.Object,
                null,
                sessionManagerMock.Object,
                null);
            var session = new SessionInfo(sessionManagerMock.Object, loggerFactoryMock.Object.CreateLogger<SessionInfo>()) { Id = "SessionId", UserName = "UserName" };

            // Act
            group.SessionLeave(session, null, default);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    "Session {SessionId} left group {GroupId}.",
                    session.Id,
                    group.GroupId.ToString()),
                Times.Once);
        }

        [Fact]
        public void HandleRequest_LogsRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Group>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var sessionManagerMock = new Mock<ISessionManager>();
            var group = new Group(
                loggerFactoryMock.Object,
                null,
                sessionManagerMock.Object,
                null);
            var session = new SessionInfo(sessionManagerMock.Object, loggerFactoryMock.Object.CreateLogger<SessionInfo>()) { Id = "SessionId" };

            // Act
            group.HandleRequest(session, null, default);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    "Session {SessionId} requested {RequestType} in group {GroupId} that is {StateType}.",
                    session.Id,
                    It.IsAny<string>(),
                    group.GroupId.ToString(),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
